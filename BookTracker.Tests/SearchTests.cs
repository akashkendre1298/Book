using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Models;
using BookTracker.Api.Models.Dtos;
using BookTracker.Api.Controllers;
using System.Net.Http.Headers;

namespace BookTracker.Tests;

public class SearchTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SearchTests(WebApplicationFactory<Program> factory)
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
    public async Task Search_ByTitle_ReturnsMatches() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "Unique Title", Author = "Author" });
        var response = await _client.GetAsync("/api/v1/books?query=Unique");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Book>>(TestJsonOptions.Options);
        result!.Items.Should().HaveCount(1);
    }

    [Fact] 
    public async Task Search_ByAuthor_Works() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "Book", Author = "Specific Author" });
        var response = await _client.GetAsync("/api/v1/books?query=Specific");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Book>>(TestJsonOptions.Options);
        result!.Items.Should().Contain(b => b.Author == "Specific Author");
    }

    [Fact] 
    public async Task Filter_ByStatus_ReturnsMatches() 
    {
        await AuthenticateAsync();
        // Create a book and then update its status to Read
        var createResponse = await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "B2", Author = "A2" });
        var book = await createResponse.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Options);
        await _client.PatchAsJsonAsync($"/api/v1/books/{book!.Id}/status", new { status = ReadingStatus.Read });

        var response = await _client.GetAsync($"/api/v1/books?status={ReadingStatus.Read}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Book>>(TestJsonOptions.Options);
        result!.Items.Should().Contain(b => b.Title == "B2");
    }

    [Fact] 
    public async Task Filter_ByGenre_ReturnsMatches() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "SciFi Book", Author = "A", Genre = "Sci-Fi" });
        var response = await _client.GetAsync("/api/v1/books?genre=Sci-Fi");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Book>>(TestJsonOptions.Options);
        result!.Items.Should().HaveCount(1);
    }

    [Fact] 
    public async Task Filter_ByStatusAndGenre_ReturnsMatches() 
    {
        await AuthenticateAsync();
        
        var res1 = await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "B1", Author = "A", Genre = "G1" });
        var b1 = await res1.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Options);
        await _client.PatchAsJsonAsync($"/api/v1/books/{b1!.Id}/status", new { status = ReadingStatus.Read });

        await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "B2", Author = "A", Genre = "G1" });
        
        var response = await _client.GetAsync($"/api/v1/books?status={ReadingStatus.Read}&genre=G1");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Book>>(TestJsonOptions.Options);
        result!.Items.Should().HaveCount(1);
    }

    [Fact] 
    public async Task Sort_ByRating_ReturnsDescending() 
    {
        await AuthenticateAsync();
        var res1 = await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "High", Author = "A" });
        var b1 = await res1.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Options);
        await _client.PatchAsJsonAsync($"/api/v1/books/{b1!.Id}/rating", new { rating = 5 });

        var response = await _client.GetAsync("/api/v1/books?sortBy=rating");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Book>>(TestJsonOptions.Options);
        result!.Items.First().Rating.Should().Be(5);
    }

    [Fact] 
    public async Task Sort_ByDate_Works() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "First", Author = "A" });
        await Task.Delay(100); // Ensure different timestamps
        await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "Second", Author = "A" });
        
        var response = await _client.GetAsync("/api/v1/books?sortBy=date");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Book>>(TestJsonOptions.Options);
        result!.Items.First().Title.Should().Be("Second"); // Descending by date
    }

    [Fact] 
    public async Task Search_SpecialCharacters_Works() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "C#", Author = "A" });
        var response = await _client.GetAsync("/api/v1/books?query=C%23");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Book>>(TestJsonOptions.Options);
        result!.Items.Should().Contain(b => b.Title == "C#");
    }

    [Fact] 
    public async Task Sort_ByTitle_Works() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "A", Author = "A" });
        await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "Z", Author = "A" });
        
        var response = await _client.GetAsync("/api/v1/books?sortBy=title");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Book>>(TestJsonOptions.Options);
        result!.Items.First().Title.Should().Be("A");
    }
}

