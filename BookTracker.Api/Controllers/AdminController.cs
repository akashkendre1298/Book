using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BookTracker.Api.Data;
using BookTracker.Api.Models;

namespace BookTracker.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : BaseApiController
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves global statistics for the entire Athenaeum platform.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetGlobalStats()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalBooks = await _context.Books.CountAsync();
        
        var genreDistribution = await _context.Books
            .GroupBy(b => b.Genre)
            .Select(g => new { Genre = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Genre, x => x.Count);

        return Ok(new
        {
            totalUsers,
            totalBooks,
            genreDistribution,
            systemHealth = "Optimal",
            lastCheck = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Retrieves a list of all registered archivists.
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Select(u => new UserProfile
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            })
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Promotes a user to Admin status.
    /// </summary>
    [HttpPost("users/{id}/promote")]
    public async Task<IActionResult> PromoteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.Role = "Admin";
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"User {user.Email} promoted to Admin status." });
    }
}
