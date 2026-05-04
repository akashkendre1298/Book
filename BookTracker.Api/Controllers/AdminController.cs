using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BookTracker.Api.Data;
using BookTracker.Api.Models;
using BookTracker.Api.Services;
using System.Text.Json;

namespace BookTracker.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : BaseApiController
{
    private readonly AppDbContext _context;
    private readonly IBookService _bookService;

    public AdminController(AppDbContext context, IBookService bookService)
    {
        _context = context;
        _bookService = bookService;
    }

    #region Dashboard
    [HttpGet("stats")]
    public async Task<IActionResult> GetGlobalStats()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalBooks = await _context.Books.CountAsync();
        var pendingRecommends = await _context.BookRecommendations.CountAsync(r => r.Status == ModerationStatus.Pending);

        return Ok(new
        {
            totalUsers,
            totalBooks,
            pendingRecommendations = pendingRecommends
        });
    }
    #endregion

    #region User Management
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Select(u => new 
            {
                u.Id,
                u.Email,
                u.Role,
                u.IsActive,
                u.LastActiveAt,
                u.CreatedAt,
                TotalUploads = _context.Books.Count(b => b.UserId == u.Id)
            })
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return Ok(users);
    }

    [HttpPut("users/{id}/status")]
    public async Task<IActionResult> ToggleUserStatus(Guid id, [FromBody] bool isActive)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsActive = isActive;
        await _context.SaveChangesAsync();

        await LogAction(isActive ? "UserUnblocked" : "UserBlocked", $"Target: {user.Email}");
        return Ok(new { message = $"User status updated to {(isActive ? "Active" : "Blocked")}" });
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        var email = user.Email;
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        await LogAction("UserDeleted", $"Deleted archivist: {email}");
        return NoContent();
    }
    #endregion

    #region Library Management
    [HttpGet("library")]
    public async Task<IActionResult> GetAllBooks()
    {
        var books = await _context.Books
            .Include(b => b.User)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new {
                b.Id,
                b.Title,
                b.Author,
                b.ISBN,
                b.Genre,
                b.TotalPages,
                b.PublicationYear,
                b.CoverImageUrl,
                b.Visibility,
                b.ModerationStatus,
                b.IsApproved,
                OwnerEmail = b.User != null ? b.User.Email : "Unknown",
                b.CreatedAt
            })
            .ToListAsync();
        return Ok(books);
    }

    [HttpPatch("library/{id}")]
    public async Task<IActionResult> PatchBookDetails(Guid id, [FromBody] JsonElement update)
    {
        var book = await _bookService.PatchBookAsync(id, update, Guid.Empty, "Admin");
        if (book == null) return NotFound();

        await LogAction("BookPatchedByAdmin", $"Title: {book.Title}");
        return Ok(book);
    }

    [HttpDelete("library/{id}")]
    public async Task<IActionResult> DeleteBook(Guid id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return NotFound();

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        await LogAction("BookDeletedByAdmin", $"Title: {book.Title}");
        return NoContent();
    }
    #endregion

    #region Recommendation Moderation
    [HttpGet("recommendations")]
    public async Task<IActionResult> GetPendingRecommendations([FromServices] IBookService bookService)
    {
        var recs = await bookService.GetPendingRecommendationsAsync();
        return Ok(recs.Select(r => new {
            r.Id, // Recommendation ID
            BookId = r.BookId,
            r.Book?.Title,
            r.Book?.Author,
            r.Book?.Genre,
            r.Book?.CoverImageUrl,
            UploadedBy = r.User?.Email ?? "Unknown",
            r.RequestedAt
        }));
    }

    [HttpPost("recommendations/{id}/approve")]
    public async Task<IActionResult> ApproveBook(Guid id, [FromServices] IBookService bookService)
    {
        var success = await bookService.HandleRecommendationAsync(id, true, "Approved by High Curator");
        if (!success) return NotFound();
        
        await LogAction("RecommendationApproved", $"RecommendationID: {id}");
        return Ok(new { message = "Book approved and added to Public Library." });
    }

    [HttpPost("recommendations/{id}/reject")]
    public async Task<IActionResult> RejectBook(Guid id, [FromServices] IBookService bookService)
    {
        var success = await bookService.HandleRecommendationAsync(id, false, "Rejected by High Curator");
        if (!success) return NotFound();

        await LogAction("RecommendationRejected", $"RecommendationID: {id}");
        return Ok(new { message = "Book rejected." });
    }
    #endregion

    private async Task LogAction(string action, string details)
    {
        var adminEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "System";
        _context.AuditLogs.Add(new AuditLog
        {
            Action = action,
            PerformedBy = adminEmail,
            Details = details
        });
        await _context.SaveChangesAsync();
    }
}
