using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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

    #region API Tests
    [Fact] 
    public async Task Search_ByTitle_ReturnsMatches() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "Unique Title", Author = "Author" });
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "Other", Author = "Author" });

        var response = await _client.GetAsync("/api/v1/books?query=Unique");
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        books.Should().HaveCount(1);
        books![0].Title.Should().Be("Unique Title");
    }

    [Fact] 
    public async Task Search_ByAuthor_ReturnsMatches() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "T1", Author = "Special Author" });
        
        var response = await _client.GetAsync("/api/v1/books?query=Special");
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        books.Should().Contain(b => b.Author == "Special Author");
    }

    [Fact] 
    public async Task Search_ShouldBeCaseInsensitive() 
    {
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "CASE", Author = "Author" });
        
        var response = await _client.GetAsync("/api/v1/books?query=case");
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        books.Should().HaveCount(1);
    }
    #endregion
}
