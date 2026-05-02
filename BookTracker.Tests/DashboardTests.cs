using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace BookTracker.Tests;

public class DashboardTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DashboardTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    #region Unit Tests
    [Fact] public void CalculateTotalPages_SumCorrectly() => Assert.Fail("Not implemented");
    [Fact] public void CalculateGenreDistribution_GroupByGenre() => Assert.Fail("Not implemented");
    [Fact] public void CalculateGoalProgress_PercentageCorrect() => Assert.Fail("Not implemented");
    #endregion

    #region API Tests
    [Fact] public async Task GetStats_ReturnsSummary() => (await _client.GetAsync("/api/v1/dashboard/stats")).StatusCode.Should().Be(HttpStatusCode.OK);
    [Fact] public async Task SetGoal_Valid_Returns200() => Assert.Fail("Not implemented");
    [Fact] public async Task ExportCSV_ReturnsFileStream() => Assert.Fail("Not implemented");
    #endregion

    #region Edge Cases
    [Fact] public async Task Stats_WithZeroBooks_ReturnsEmptyStructure() => Assert.Fail("Not implemented");
    #endregion
}
