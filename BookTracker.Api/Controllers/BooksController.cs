using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookTracker.Api.Data;
using BookTracker.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace BookTracker.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly AppDbContext _context;

    public BooksController(AppDbContext context)
    {
        _context = context;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? query)
    {
        var queryable = _context.Books.Where(b => b.UserId == UserId);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLower();
            queryable = queryable.Where(b => 
                (b.Title != null && b.Title.ToLower().Contains(lowerQuery)) || 
                (b.Author != null && b.Author.ToLower().Contains(lowerQuery)) || 
                (b.ISBN != null && b.ISBN.ToLower().Contains(lowerQuery)));
        }

        var books = await queryable.ToListAsync();
        return Ok(books);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId);
        if (book == null) return NotFound();
        return Ok(book);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Book book)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        book.UserId = UserId;
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Book bookUpdate)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId);
        if (book == null) return NotFound();

        book.Title = bookUpdate.Title;
        book.Author = bookUpdate.Author;
        book.ISBN = bookUpdate.ISBN;
        book.Genre = bookUpdate.Genre;
        book.TotalPages = bookUpdate.TotalPages;
        book.PublicationYear = bookUpdate.PublicationYear;
        book.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(book);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId);
        if (book == null) return NotFound();

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/cover")]
    public async Task<IActionResult> UploadCover(Guid id, IFormFile file)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId);
        if (book == null) return NotFound();

        if (file.Length > 5 * 1024 * 1024) return BadRequest("File too large (max 5MB)");
        var allowedTypes = new[] { "image/jpeg", "image/png" };
        if (!allowedTypes.Contains(file.ContentType)) return BadRequest("Invalid file type");

        book.CoverImageUrl = $"/uploads/{Guid.NewGuid()}_{file.FileName}";
        await _context.SaveChangesAsync();

        return Ok(new { url = book.CoverImageUrl });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> PatchStatus(Guid id, [FromBody] JsonElement body)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId);
        if (book == null) return NotFound();

        if (body.TryGetProperty("status", out var statusProp))
        {
            book.Status = (ReadingStatus)statusProp.GetInt32();
            book.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(book);
        }
        return BadRequest();
    }

    [HttpPatch("{id}/progress")]
    public async Task<IActionResult> PatchProgress(Guid id, [FromBody] JsonElement body)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId);
        if (book == null) return NotFound();

        if (body.TryGetProperty("currentPage", out var progressProp))
        {
            var progress = progressProp.GetInt32();
            if (progress < 0 || (book.TotalPages.HasValue && progress > book.TotalPages.Value))
                return BadRequest("Invalid progress value");

            book.CurrentPage = progress;
            if (book.TotalPages.HasValue && progress == book.TotalPages.Value)
                book.Status = ReadingStatus.Read;

            book.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(book);
        }
        return BadRequest();
    }

    [HttpPatch("{id}/rating")]
    public async Task<IActionResult> PatchRating(Guid id, [FromBody] JsonElement body)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId);
        if (book == null) return NotFound();

        if (body.TryGetProperty("rating", out var ratingProp))
        {
            var rating = ratingProp.GetInt32();
            if (rating < 1 || rating > 5) return BadRequest("Rating must be between 1 and 5");

            book.Rating = rating;
            book.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(book);
        }
        return BadRequest();
    }
}
