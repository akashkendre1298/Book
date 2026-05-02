using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace BookTracker.Tests;

public class ReadingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ReadingTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    #region Unit Tests
    [Fact] public void Progress_MustBeBetweenZeroAndTotalPages() => Assert.Fail("Not implemented");
    [Fact] public void Rating_MustBeBetweenOneAndFive() => Assert.Fail("Not implemented");
    #endregion

    #region API Tests
    [Fact] public async Task UpdateStatus_Valid_Returns200() => Assert.Fail("Not implemented");
    [Fact] public async Task UpdateProgress_Valid_Returns200() => Assert.Fail("Not implemented");
    [Fact] public async Task UpdateProgress_Invalid_Returns400() => Assert.Fail("Not implemented");
    [Fact] public async Task MarkAsCompleted_SetsFinishDate() => Assert.Fail("Not implemented");
    [Fact] public async Task AddRating_Valid_Returns200() => Assert.Fail("Not implemented");
    #endregion

    #region Edge Cases
    [Fact] public async Task Progress_AtMax_Succeeds() => Assert.Fail("Not implemented");
    #endregion
}
