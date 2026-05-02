using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Models;
using BookTracker.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

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
        
        var scope = factory.Services.CreateScope();
        _authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    }

    #region Unit Tests
    [Fact] 
    public void PasswordHashing_ShouldBeSecure() 
    {
        var password = "SecurePassword123!";
        var hash = _authService.HashPassword(password);
        hash.Should().NotBe(password);
    }

    [Fact] 
    public void PasswordVerification_ShouldMatch() 
    {
        var password = "SecurePassword123!";
        var hash = _authService.HashPassword(password);
        _authService.ComparePassword(password, hash).Should().BeTrue();
    }

    [Fact] 
    public void EmailValidation_ShouldRejectInvalid() 
    {
        _authService.ValidateEmail("invalid-email").Should().BeFalse();
        _authService.ValidateEmail("valid@test.com").Should().BeTrue();
    }

    [Fact] 
    public void PasswordStrength_ShouldBeValidated() 
    {
        _authService.ValidatePassword("simple").Should().BeFalse();
        _authService.ValidatePassword("StrongPass123!").Should().BeTrue();
    }
    #endregion

    #region API Integration Tests
    [Fact] 
    public async Task Register_WithValidData_Returns201() 
    {
        var email = $"newuser_{Guid.NewGuid()}@test.com";
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = email, Password = "Password123!" });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact] 
    public async Task Register_InvalidEmail_Returns400() 
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = "invalid", Password = "Password123!" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact] 
    public async Task Register_ShortPassword_Returns400() 
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = "valid@test.com", Password = "123" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact] 
    public async Task Register_DuplicateEmail_Returns409() 
    {
        var email = "duplicate@test.com";
        await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = email, Password = "Password123!" });
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = email, Password = "Password123!" });
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact] 
    public async Task Login_ValidCredentials_Returns200() 
    {
        var email = $"login_{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = email, Password = "Password123!" });
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Password123!" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact] 
    public async Task Login_InvalidCredentials_Returns401() 
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { Email = "wrong@test.com", Password = "WrongPassword" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact] 
    public async Task AccessProtected_WithoutToken_Returns401() 
    {
        var response = await _client.GetAsync("/api/v1/books");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    #endregion

    #region Edge Cases
    [Fact] 
    public async Task Email_ShouldBeCaseNormalized() 
    {
        var email = $"CASE_{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = email.ToUpper(), Password = "Password123!" });
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email.ToLower(), Password = "Password123!" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact] 
    public async Task Logout_Returns204() 
    {
        var response = await _client.PostAsync("/api/v1/auth/logout", null);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    #endregion
}
