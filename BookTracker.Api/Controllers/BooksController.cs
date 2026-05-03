using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookTracker.Api.Models;
using BookTracker.Api.Services;
using BookTracker.Api.Constants;
using System.Text.Json;

namespace BookTracker.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class BooksController : BaseApiController
{
    private readonly IBookService _bookService;
    private readonly IWebHostEnvironment _env;

    public BooksController(IBookService bookService, IWebHostEnvironment env)
    {
        _bookService = bookService;
        _env = env;
    }

    /// <summary>
    /// Retrieves all books for the authenticated user with optional filtering and sorting.
    /// </summary>
    /// <param name="query">Search term for title or author</param>
    /// <param name="status">Filter by reading status</param>
    /// <param name="genre">Filter by genre</param>
    /// <param name="rating">Filter by star rating</param>
    /// <param name="sortBy">Sort field (title, author, rating, dateadded)</param>
    /// <returns>A list of books</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Book>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? query, 
        [FromQuery] ReadingStatus? status, 
        [FromQuery] string? genre,
        [FromQuery] int? rating,
        [FromQuery] string? sortBy = ApiConstants.DefaultSortBy)
    {
        var books = await _bookService.GetBooksAsync(UserId, query, status, genre, rating, sortBy);
        return Ok(books);
    }

    /// <summary>
    /// Retrieves a specific book by its ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var book = await _bookService.GetBookByIdAsync(id, UserId);
        if (book == null) return NotFound();
        return Ok(book);
    }

    /// <summary>
    /// Adds a new book to the user's collection.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Book), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] Book book)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var createdBook = await _bookService.CreateBookAsync(book, UserId);
            return CreatedAtAction(nameof(GetById), new { id = createdBook.Id }, createdBook);
        }
        catch (InvalidOperationException ex) when (ex.Message == "DuplicateISBN")
        {
            return Conflict(new { message = "Book with this ISBN already exists in your collection" });
        }
    }

    /// <summary>
    /// Updates the core details of an existing book.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] Book bookUpdate)
    {
        try
        {
            var book = await _bookService.UpdateBookAsync(id, bookUpdate, UserId);
            if (book == null) return NotFound();
            return Ok(book);
        }
        catch (InvalidOperationException ex) when (ex.Message == "DuplicateISBN")
        {
            return Conflict(new { message = "Book with this ISBN already exists in your collection" });
        }
    }

    /// <summary>
    /// Removes a book from the user's collection permanently.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _bookService.DeleteBookAsync(id, UserId);
        if (!deleted) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Uploads a cover image for a specific book.
    /// </summary>
    [HttpPost("{id}/cover")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadCover(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded");

        if (file.Length > ApiConstants.MaxCoverImageSize) 
            return BadRequest($"File too large (max {ApiConstants.MaxCoverImageSize / (1024 * 1024)}MB)");
        
        if (!ApiConstants.AllowedCoverImageTypes.Contains(file.ContentType)) 
            return BadRequest("Invalid file type. Allowed: .jpg, .jpeg, .png");

        var book = await _bookService.GetBookByIdAsync(id, UserId);
        if (book == null) return NotFound();

        // Create unique filename
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        
        var folderPath = Path.Combine(_env.ContentRootPath, "uploads");
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        
        var filePath = Path.Combine(folderPath, fileName);

        // Save to disk
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Update database with relative URL
        var url = $"/uploads/{fileName}";
        await _bookService.UpdateCoverAsync(id, url, UserId);

        return Ok(new { url });
    }

    /// <summary>
    /// Updates the reading status (Want to Read, Reading, Read).
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PatchStatus(Guid id, [FromBody] JsonElement body)
    {
        if (body.TryGetProperty("status", out var statusProp))
        {
            var book = await _bookService.UpdateStatusAsync(id, (ReadingStatus)statusProp.GetInt32(), UserId);
            if (book == null) return NotFound();
            return Ok(book);
        }
        return BadRequest();
    }

    /// <summary>
    /// Updates the current reading progress (page number).
    /// </summary>
    [HttpPatch("{id}/progress")]
    [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PatchProgress(Guid id, [FromBody] JsonElement body)
    {
        if (body.TryGetProperty("currentPage", out var progressProp))
        {
            var progress = progressProp.GetInt32();
            var book = await _bookService.GetBookByIdAsync(id, UserId);
            if (book == null) return NotFound();

            if (progress < 0 || (book.TotalPages.HasValue && progress > book.TotalPages.Value))
                return BadRequest("Invalid progress value");

            var updatedBook = await _bookService.UpdateProgressAsync(id, progress, UserId);
            return Ok(updatedBook);
        }
        return BadRequest();
    }

    /// <summary>
    /// Updates the star rating of a book.
    /// </summary>
    [HttpPatch("{id}/rating")]
    [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PatchRating(Guid id, [FromBody] JsonElement body)
    {
        if (body.TryGetProperty("rating", out var ratingProp))
        {
            var rating = ratingProp.GetInt32();
            if (rating < 1 || rating > 5) return BadRequest("Rating must be between 1 and 5");

            var book = await _bookService.UpdateRatingAsync(id, rating, UserId);
            if (book == null) return NotFound();
            return Ok(book);
        }
        return BadRequest();
    }

    /// <summary>
    /// Exports the entire collection as a CSV file.
    /// </summary>
    /// <returns>A CSV file stream</returns>
    [HttpGet("export")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Export()
    {
        var csvBytes = await _bookService.ExportCsvAsync(UserId);
        return File(csvBytes, "text/csv", $"athenaeum_export_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
