using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Models;
using System.Net.Http.Headers;

namespace BookTracker.Tests;

public class ReadingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ReadingTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder => {
            builder.UseEnvironment("Testing");
        }).CreateClient();
    }

    private async Task AuthenticateAsync()
    {
        var email = $"user_{Guid.NewGuid()}@test.com";
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = email, Password = "Password123!" });
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);
    }

    [Fact] 
    public async Task UpdateStatus_Valid_Returns200() 
    {
        await AuthenticateAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "Book", Author = "Author" });
        var book = await createResponse.Content.ReadFromJsonAsync<Book>();
        var response = await _client.PatchAsJsonAsync($"/api/v1/books/{book!.Id}/status", new { status = ReadingStatus.Reading });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact] 
    public async Task UpdateProgress_Valid_Returns200() 
    {
        await AuthenticateAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "Book", Author = "Author", TotalPages = 100 });
        var book = await createResponse.Content.ReadFromJsonAsync<Book>();
        var response = await _client.PatchAsJsonAsync($"/api/v1/books/{book!.Id}/progress", new { currentPage = 50 });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact] 
    public async Task UpdateProgress_Invalid_Returns400() 
    {
        await AuthenticateAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "Book", Author = "Author", TotalPages = 100 });
        var book = await createResponse.Content.ReadFromJsonAsync<Book>();
        var response = await _client.PatchAsJsonAsync($"/api/v1/books/{book!.Id}/progress", new { currentPage = 150 });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact] 
    public async Task AddRating_Valid_Returns200() 
    {
        await AuthenticateAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "Book", Author = "Author" });
        var book = await createResponse.Content.ReadFromJsonAsync<Book>();
        var response = await _client.PatchAsJsonAsync($"/api/v1/books/{book!.Id}/rating", new { rating = 5 });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact] 
    public async Task UpdateRating_MultipleTimes_Works() 
    {
        await AuthenticateAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "Book", Author = "Author" });
        var book = await createResponse.Content.ReadFromJsonAsync<Book>();
        await _client.PatchAsJsonAsync($"/api/v1/books/{book!.Id}/rating", new { rating = 1 });
        var response = await _client.PatchAsJsonAsync($"/api/v1/books/{book.Id}/rating", new { rating = 5 });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact] 
    public async Task AutoSet_FinishDate_WhenRead() 
    {
        await AuthenticateAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "Book", Author = "Author", TotalPages = 100 });
        var book = await createResponse.Content.ReadFromJsonAsync<Book>();
        await _client.PatchAsJsonAsync($"/api/v1/books/{book!.Id}/progress", new { currentPage = 100 });
        var getResponse = await _client.GetAsync($"/api/v1/books/{book.Id}");
        var updatedBook = await getResponse.Content.ReadFromJsonAsync<Book>();
        updatedBook!.Status.Should().Be(ReadingStatus.Read);
    }

    [Fact] 
    public async Task Progress_ZeroToHundred_Directly() 
    {
        await AuthenticateAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "Book", Author = "Author", TotalPages = 100, CurrentPage = 0 });
        var book = await createResponse.Content.ReadFromJsonAsync<Book>();
        await _client.PatchAsJsonAsync($"/api/v1/books/{book!.Id}/progress", new { currentPage = 100 });
        var getResponse = await _client.GetAsync($"/api/v1/books/{book.Id}");
        var updatedBook = await getResponse.Content.ReadFromJsonAsync<Book>();
        updatedBook!.CurrentPage.Should().Be(100);
    }
}
