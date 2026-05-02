using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookTracker.Api.Data;
using BookTracker.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookTracker.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var books = await _context.Books.Where(b => b.UserId == UserId).ToListAsync();
        
        var stats = new
        {
            totalBooks = books.Count,
            readBooks = books.Count(b => b.Status == ReadingStatus.Read),
            readingBooks = books.Count(b => b.Status == ReadingStatus.Reading),
            totalPagesRead = books.Sum(b => b.CurrentPage),
            genreDistribution = books.GroupBy(b => b.Genre ?? "Uncategorized")
                                     .ToDictionary(g => g.Key, g => g.Count())
        };

        return Ok(stats);
    }

    [HttpPost("goal")]
    public async Task<IActionResult> SetGoal([FromBody] ReadingGoal goal)
    {
        var existing = await _context.ReadingGoals.FirstOrDefaultAsync(g => g.UserId == UserId && g.TargetYear == goal.TargetYear);
        if (existing != null)
        {
            existing.GoalCount = goal.GoalCount;
        }
        else
        {
            goal.UserId = UserId;
            _context.ReadingGoals.Add(goal);
        }

        await _context.SaveChangesAsync();
        return Ok(goal);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportCsv()
    {
        var books = await _context.Books.Where(b => b.UserId == UserId).ToListAsync();
        
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Title,Author,ISBN,Genre,Status,CurrentPage,TotalPages,Rating");

        foreach (var book in books)
        {
            csv.AppendLine($"{book.Title},{book.Author},{book.ISBN},{book.Genre},{book.Status},{book.CurrentPage},{book.TotalPages},{book.Rating}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", "books.csv");
    }
}
