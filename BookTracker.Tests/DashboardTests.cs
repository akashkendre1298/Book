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
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);
    }

    #region API Tests
    [Fact] 
    public async Task GetStats_ReturnsSummary() 
    {
        await AuthenticateAsync();
        // Add some data
        await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "B1", Author = "A1", TotalPages = 100, Status = ReadingStatus.Read });
        
        var response = await _client.GetAsync("/api/v1/dashboard/stats");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Structure check (simulated)
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("totalBooks");
    }

    [Fact] 
    public async Task SetGoal_Valid_Returns200() 
    {
        await AuthenticateAsync();
        var response = await _client.PostAsJsonAsync("/api/v1/dashboard/goal", new { targetYear = 2026, goalCount = 12 });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    #endregion
}
