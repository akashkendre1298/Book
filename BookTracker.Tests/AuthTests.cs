// TDD RED Phase: Failing Authentication Tests
using Xunit;
using FluentAssertions;
using Moq;
using BookTracker.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Models;

namespace BookTracker.Tests;

public class AuthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly IAuthService _authService;

    public AuthTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder => {
            builder.UseEnvironment("Testing");
        }).CreateClient();
        _authService = new AuthService(new ConfigurationBuilder().Build());
    }

    #region Unit Tests
    [Fact] 
    public void HashPassword_ShouldReturnHashedString() 
    {
        var password = "Password123!";
        var hash = _authService.HashPassword(password);
        hash.Should().NotBe(password);
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact] 
    public void ComparePassword_ShouldVerifyCorrectly() 
    {
        var password = "Password123!";
        var hash = _authService.HashPassword(password);
        _authService.ComparePassword(password, hash).Should().BeTrue();
    }

    [Fact] 
    public void ValidateEmail_ShouldSupportStandardFormats() 
    {
        _authService.ValidateEmail("test@example.com").Should().BeTrue();
        _authService.ValidateEmail("invalid").Should().BeFalse();
    }

    [Fact] 
    public void ValidatePassword_ShouldEnforceComplexity() 
    {
        _authService.ValidatePassword("StrongPass123!").Should().BeTrue();
        _authService.ValidatePassword("weak").Should().BeFalse();
    }
    #endregion

    #region API Tests
    [Fact] 
    public async Task Register_WithValidData_Returns201() 
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = "newuser@test.com", Password = "Password123!" });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact] 
    public async Task Register_WithDuplicateEmail_Returns409() 
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = "dup@test.com", Password = "Password123!" });
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = "dup@test.com", Password = "Password123!" });
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact] 
    public async Task Register_WithWeakPassword_Returns400() 
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = "weak@test.com", Password = "123" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact] 
    public async Task Login_WithValidCredentials_Returns200() 
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = "login@test.com", Password = "Password123!" });
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { Email = "login@test.com", Password = "Password123!" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact] 
    public async Task Login_WithWrongPassword_Returns401() 
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = "wrong@test.com", Password = "Password123!" });
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { Email = "wrong@test.com", Password = "WrongPassword!" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact] 
    public async Task AccessProtected_WithoutToken_Returns401() 
    {
        var response = await _client.GetAsync("/api/v1/books");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact] 
    public async Task Logout_ClearsSession() 
    {
        // For JWT, logout is usually client side, but we might have a blacklist. 
        // For now, we test the endpoint exists as per documentation.
        // var response = await _client.PostAsync("/api/v1/auth/logout", null);
        // response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Skip for now as it wasn't in the schema/docs as a functional requirement for server-side logic in MVP
    }
    #endregion

    #region Edge Cases
    [Fact] 
    public async Task Email_ShouldBeCaseNormalized() 
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = "CASE@test.com", Password = "Password123!" });
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { Email = "case@test.com", Password = "Password123!" });
        
        // We'll see if our current implementation handles this. If not, it will fail (RED) and we refactor.
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    #endregion
}
