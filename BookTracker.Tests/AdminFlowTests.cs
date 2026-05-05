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

public class AdminFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AdminFlowTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder => {
            builder.UseEnvironment("Testing");
        }).CreateClient();
    }

    private async Task<string> AuthenticateAsAdminAsync()
    {
        var adminEmail = "curator@athenaeum.com"; 
        await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = adminEmail, Password = "CuratorPassword123!" });
        var loginRes = await _client.PostAsJsonAsync("/api/v1/auth/login", new { Email = adminEmail, Password = "CuratorPassword123!" });
        var auth = await loginRes.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Options);
        return auth!.Token;
    }

    private async Task<string> AuthenticateAsUserAsync()
    {
        var email = $"user_{Guid.NewGuid()}@test.com";
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = email, Password = "UserPassword123!" });
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Options);
        return result!.Token;
    }

    [Fact]
    public async Task AdminCreatedBook_ShouldBePublicAndApprovedByDefault()
    {
        // 1. Arrange: Login as Admin
        var token = await AuthenticateAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 2. Act: Add a new book
        var bookDto = new BookCreateDto { Title = "Admin's Masterpiece", Author = "High Curator" };
        var response = await _client.PostAsJsonAsync("/api/v1/books", bookDto);
        
        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Options);
        created!.Visibility.Should().Be(BookVisibility.Public);
        created.IsApproved.Should().BeTrue();
        created.ModerationStatus.Should().Be(ModerationStatus.Approved);
    }

    [Fact]
    public async Task RegularUserCreatedBook_ShouldBePrivateAndUnapprovedByDefault()
    {
        // 1. Arrange: Login as Regular User
        var token = await AuthenticateAsUserAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 2. Act: Add a new book
        var bookDto = new BookCreateDto { Title = "My Private Journal", Author = "Normal User" };
        var response = await _client.PostAsJsonAsync("/api/v1/books", bookDto);

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Options);
        created!.Visibility.Should().Be(BookVisibility.Private);
        created.IsApproved.Should().Be(false);
        created.ModerationStatus.Should().Be(ModerationStatus.None);
    }

    [Fact]
    public async Task PublicLibrary_ShouldReturnOnlyApprovedPublicBooks()
    {
        // 1. Arrange
        var adminToken = await AuthenticateAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "Public Approved", Author = "Admin" });
        
        var userToken = await AuthenticateAsUserAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "Private User", Author = "User" });

        // 2. Act
        var response = await _client.GetAsync("/api/v1/books/public");
        
        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Book>>(TestJsonOptions.Options);
        result!.Items.Should().AllSatisfy(b => {
            b.Visibility.Should().Be(BookVisibility.Public);
            b.IsApproved.Should().BeTrue();
        });
        result.Items.Should().Contain(b => b.Title == "Public Approved");
        result.Items.Should().NotContain(b => b.Title == "Private User");
    }

    [Fact]
    public async Task PublicLibrary_ShouldBeStateNeutral()
    {
        // 1. Arrange
        var adminToken = await AuthenticateAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var res = await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "Neutrality Check", Author = "Admin" });
        var book = await res.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Options);
        
        // Set some state as admin (owner)
        await _client.PatchAsJsonAsync($"/api/v1/books/{book!.Id}/status", new { status = ReadingStatus.Reading });
        await _client.PatchAsJsonAsync($"/api/v1/books/{book.Id}/progress", new { currentPage = 50 });

        // 2. Act: Fetch from public view
        var response = await _client.GetAsync("/api/v1/books/public");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Book>>(TestJsonOptions.Options);
        var publicBook = result!.Items.First(b => b.Id == book.Id);

        // 3. Assert: Progress/Status/Rating should be reset for ALL books in public view
        publicBook.Status.Should().Be(ReadingStatus.WantToRead);
        publicBook.CurrentPage.Should().Be(0);
        publicBook.Rating.Should().BeNull();
    }
}

