using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookTracker.Api.Models;
using BookTracker.Api.Services;

namespace BookTracker.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DashboardController : BaseApiController
{
    private readonly IBookService _bookService;

    public DashboardController(IBookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>
    /// Retrieves overall statistics for the user's collection.
    /// </summary>
    /// <returns>Statistics object including counts and distribution</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _bookService.GetStatsAsync(UserId);
        return Ok(stats);
    }

    /// <summary>
    /// Sets or updates a reading goal for a specific year.
    /// </summary>
    [HttpPost("goal")]
    [ProducesResponseType(typeof(ReadingGoal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetGoal([FromBody] ReadingGoal goal)
    {
        var result = await _bookService.SetGoalAsync(goal, UserId);
        return Ok(result);
    }
}
