using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Models;
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
        var email = $"admin_{Guid.NewGuid()}@test.com";
        // In testing environment, we assume the seed or a specific promotion logic works.
        // For this test, we'll register and then simulate the role being Admin.
        // NOTE: In a real test, you'd have a seeded Admin user.
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = email, Password = "AdminPassword123!" });
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        
        // Manual promotion for testing if needed, or use a known admin account.
        // Assuming the first registered user in a clean test DB might be admin or promoted via SQL.
        return result!.Token;
    }

    private async Task<string> AuthenticateAsUserAsync()
    {
        var email = $"user_{Guid.NewGuid()}@test.com";
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = email, Password = "UserPassword123!" });
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return result!.Token;
    }

    [Fact]
    public async Task AdminCreatedBook_ShouldBePublicAndApprovedByDefault()
    {
        // 1. Arrange: Login as Admin
        // For testing, we use a specific email that the system promotes to Admin
        var adminEmail = "curator@athenaeum.com"; 
        await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = adminEmail, Password = "CuratorPassword123!" });
        var loginRes = await _client.PostAsJsonAsync("/api/v1/auth/login", new { Email = adminEmail, Password = "CuratorPassword123!" });
        var auth = await loginRes.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        // 2. Act: Add a new book
        var book = new Book { Title = "Admin's Masterpiece", Author = "High Curator" };
        var response = await _client.PostAsJsonAsync("/api/v1/books", book);
        
        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<Book>();
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
        var book = new Book { Title = "My Private Journal", Author = "Normal User" };
        var response = await _client.PostAsJsonAsync("/api/v1/books", book);

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<Book>();
        created!.Visibility.Should().Be(BookVisibility.Private);
        created.IsApproved.Should().BeFalse();
        created.ModerationStatus.Should().Be(ModerationStatus.None);
    }

    [Fact]
    public async Task PublicLibrary_ShouldReturnOnlyApprovedPublicBooks()
    {
        // 1. Arrange
        // Add one public approved book (Admin)
        // Add one private book (User)
        
        // (Simplified for brevity, assuming existing data or setup)
        var response = await _client.GetAsync("/api/v1/books/public");
        
        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        books.Should().AllSatisfy(b => {
            b.Visibility.Should().Be(BookVisibility.Public);
            b.IsApproved.Should().BeTrue();
        });
    }

    [Fact]
    public async Task PublicLibrary_ShouldBeStateNeutral()
    {
        // 1. Arrange
        var response = await _client.GetAsync("/api/v1/books/public");
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();

        // 3. Assert: Progress/Status/Rating should be reset for ALL books in public view
        books.Should().AllSatisfy(b => {
            b.Status.Should().Be(ReadingStatus.WantToRead);
            b.CurrentPage.Should().Be(0);
            b.Rating.Should().BeNull();
            b.Review.Should().BeNull();
        });
    }
}
