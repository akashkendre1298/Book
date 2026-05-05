using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Models;
using BookTracker.Api.Models.Dtos;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BookTracker.Tests;

public class DashboardTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DashboardTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder => {
            builder.UseEnvironment("Testing");
        }).CreateClient();
    }

    private async Task AuthenticateAsync()
    {
        var email = $"user_{Guid.NewGuid()}@test.com";
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = email, Password = "Password123!" });
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Options);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);
    }

    [Fact] 
    public async Task GetStats_ReturnsSummary() 
    {
        await AuthenticateAsync();
        var createRes = await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "B1", Author = "A1", TotalPages = 100 });
        var book = await createRes.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Options);
        
        await _client.PatchAsJsonAsync($"/api/v1/books/{book!.Id}/status", new { status = ReadingStatus.Reading });
        await _client.PatchAsJsonAsync($"/api/v1/books/{book.Id}/progress", new { currentPage = 50 });

        var response = await _client.GetAsync("/api/v1/dashboard/stats");
        var stats = await response.Content.ReadFromJsonAsync<JsonElement>();
        stats.GetProperty("totalBooks").GetInt32().Should().Be(1);
    }

    [Fact] 
    public async Task GetStats_ZeroDataset_ReturnsZeroes() 
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/v1/dashboard/stats");
        var stats = await response.Content.ReadFromJsonAsync<JsonElement>();
        stats.GetProperty("totalBooks").GetInt32().Should().Be(0);
    }

    [Fact] 
    public async Task ExportCsv_ReturnsFile() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "CSV Book", Author = "Author" });
        var response = await _client.GetAsync("/api/v1/books/export");
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
    }

    [Fact] 
    public async Task ExportCsv_Empty_ReturnsHeaderOnly() 
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/v1/books/export");
        var csv = await response.Content.ReadAsStringAsync();
        csv.Should().StartWith("Title,Author");
    }

    [Fact] 
    public async Task SetGoal_Update_Works() 
    {
        await AuthenticateAsync();
        var year = DateTime.UtcNow.Year;
        await _client.PostAsJsonAsync("/api/v1/dashboard/goal", new { targetYear = year, goalCount = 10 });
        var response = await _client.PostAsJsonAsync("/api/v1/dashboard/goal", new { targetYear = year, goalCount = 20 });
        var goal = await response.Content.ReadFromJsonAsync<ReadingGoal>(TestJsonOptions.Options);
        goal!.GoalCount.Should().Be(20);
    }

    [Fact] 
    public async Task Stats_GenreDistribution_Correct() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "B1", Author = "A", Genre = "Fantasy" });
        await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "B2", Author = "A", Genre = "Fantasy" });
        var response = await _client.GetAsync("/api/v1/dashboard/stats");
        var stats = await response.Content.ReadFromJsonAsync<JsonElement>();
        stats.GetProperty("genreDistribution").GetProperty("Fantasy").GetInt32().Should().Be(2);
    }

    [Fact] 
    public async Task Stats_HandlesLargeNumbers() 
    {
        await AuthenticateAsync();
        var createRes = await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "Large", Author = "A", TotalPages = 1000000 });
        var book = await createRes.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Options);

        await _client.PatchAsJsonAsync($"/api/v1/books/{book!.Id}/status", new { status = ReadingStatus.Reading });
        await _client.PatchAsJsonAsync($"/api/v1/books/{book.Id}/progress", new { currentPage = 500000 });

        var response = await _client.GetAsync("/api/v1/dashboard/stats");
        var stats = await response.Content.ReadFromJsonAsync<JsonElement>();
        stats.GetProperty("totalPagesRead").GetInt32().Should().Be(500000);
    }
}

