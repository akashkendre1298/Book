using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Models;
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
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);
    }

    [Fact] 
    public async Task Search_ByTitle_ReturnsMatches() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "Unique Title", Author = "Author" });
        var response = await _client.GetAsync("/api/v1/books?query=Unique");
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        books.Should().HaveCount(1);
    }

    [Fact] 
    public async Task Search_ByAuthor_Works() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "Book", Author = "Specific Author" });
        var response = await _client.GetAsync("/api/v1/books?query=Specific");
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        books.Should().Contain(b => b.Author == "Specific Author");
    }

    [Fact] 
    public async Task Filter_ByStatus_ReturnsMatches() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "B2", Author = "A2", Status = ReadingStatus.Read });
        var response = await _client.GetAsync("/api/v1/books?status=2");
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        books.Should().Contain(b => b.Title == "B2");
    }

    [Fact] 
    public async Task Filter_ByGenre_ReturnsMatches() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "SciFi Book", Author = "A", Genre = "Sci-Fi" });
        var response = await _client.GetAsync("/api/v1/books?genre=Sci-Fi");
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        books.Should().HaveCount(1);
    }

    [Fact] 
    public async Task Filter_ByStatusAndGenre_ReturnsMatches() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "B1", Author = "A", Status = ReadingStatus.Read, Genre = "G1" });
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "B2", Author = "A", Status = ReadingStatus.Reading, Genre = "G1" });
        var response = await _client.GetAsync("/api/v1/books?status=2&genre=G1");
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        books.Should().HaveCount(1);
    }

    [Fact] 
    public async Task Sort_ByRating_ReturnsDescending() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "High", Author = "A", Rating = 5 });
        var response = await _client.GetAsync("/api/v1/books?sortBy=rating");
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        books![0].Rating.Should().Be(5);
    }

    [Fact] 
    public async Task Sort_ByDate_Works() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "First", Author = "A" });
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "Second", Author = "A" });
        var response = await _client.GetAsync("/api/v1/books?sortBy=date");
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        books![0].Title.Should().Be("Second"); // Descending by date
    }

    [Fact] 
    public async Task Search_SpecialCharacters_Works() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "C#", Author = "A" });
        var response = await _client.GetAsync("/api/v1/books?query=C%23");
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        books.Should().Contain(b => b.Title == "C#");
    }

    [Fact] 
    public async Task Sort_ByTitle_Works() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "A", Author = "A" });
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "Z", Author = "A" });
        var response = await _client.GetAsync("/api/v1/books?sortBy=title");
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        books![0].Title.Should().Be("A");
    }
}
