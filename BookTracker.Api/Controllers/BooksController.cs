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
    [HttpGet("my")]
    [ProducesResponseType(typeof(IEnumerable<Book>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCollection(
        [FromQuery] string? query, 
        [FromQuery] ReadingStatus? status, 
        [FromQuery] string? sortBy = ApiConstants.DefaultSortBy)
    {
        var books = await _bookService.GetMyCollectionAsync(UserId, query, status, sortBy);
        return Ok(books);
    }

    /// <summary>
    /// Retrieves books from the public library (Approved Public volumes).
    /// </summary>
    [HttpGet("public")]
    [ProducesResponseType(typeof(IEnumerable<Book>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicLibrary(
        [FromQuery] string? query, 
        [FromQuery] string? genre,
        [FromQuery] string? sortBy = ApiConstants.DefaultSortBy)
    {
        var books = await _bookService.GetPublicLibraryAsync(query, genre, sortBy);
        return Ok(books);
    }

    /// <summary>
    /// Retrieves only the user's completed volumes.
    /// </summary>
    [HttpGet("completed")]
    [ProducesResponseType(typeof(IEnumerable<Book>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompleted(
        [FromQuery] string? query, 
        [FromQuery] string? sortBy = ApiConstants.DefaultSortBy)
    {
        var books = await _bookService.GetCompletedBooksAsync(UserId, query, sortBy);
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
        var book = await _bookService.GetBookByIdAsync(id, UserId, UserRole);
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
            var book = await _bookService.UpdateBookAsync(id, bookUpdate, UserId, UserRole);
            if (book == null) return NotFound();
            return Ok(book);
        }
        catch (InvalidOperationException ex) when (ex.Message == "DuplicateISBN")
        {
            return Conflict(new { message = "Book with this ISBN already exists in your collection" });
        }
    }

    /// <summary>
    /// Performs a partial update on a book's archival record.
    /// </summary>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Patch(Guid id, [FromBody] JsonElement body)
    {
        var book = await _bookService.PatchBookAsync(id, body, UserId, UserRole);
        if (book == null) return NotFound();
        return Ok(book);
    }

    /// <summary>
    /// Removes a book from the user's collection permanently.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _bookService.DeleteBookAsync(id, UserId, UserRole);
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
        
        var book = await _bookService.GetBookByIdAsync(id, UserId, UserRole);
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
        await _bookService.UpdateCoverAsync(id, url, UserId, UserRole);

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
            var book = await _bookService.UpdateStatusAsync(id, (ReadingStatus)statusProp.GetInt32(), UserId, UserRole);
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
            var book = await _bookService.GetBookByIdAsync(id, UserId, UserRole);
            if (book == null) return NotFound();

            if (progress < 0 || (book.TotalPages.HasValue && progress > book.TotalPages.Value))
                return BadRequest("Invalid progress value");
            
            var updatedBook = await _bookService.UpdateProgressAsync(id, progress, UserId, UserRole);
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
            
            var book = await _bookService.UpdateRatingAsync(id, rating, UserId, UserRole);
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

    /// <summary>
    /// Uploads a complete digital volume (Metadata + PDF + Cover).
    /// </summary>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(Book), StatusCodes.Status201Created)]
    public async Task<IActionResult> UploadBook(
        [FromForm] IFormFile? pdf, 
        [FromForm] IFormFile? cover, 
        [FromForm] string title, 
        [FromForm] string author, 
        [FromForm] string? genre, 
        [FromForm] string? isbn,
        [FromForm] int? totalPages,
        [FromForm] int? publicationYear,
        [FromForm] bool isPublic)
    {
        // 1. Process PDF (Optional)
        string? pdfUrl = null;
        if (pdf != null && pdf.Length > 0)
        {
            var pdfExt = Path.GetExtension(pdf.FileName);
            var pdfName = $"{Guid.NewGuid()}{pdfExt}";
            var pdfFolder = Path.Combine(_env.ContentRootPath, "uploads", "books");
            if (!Directory.Exists(pdfFolder)) Directory.CreateDirectory(pdfFolder);
            var pdfPath = Path.Combine(pdfFolder, pdfName);

            using (var stream = new FileStream(pdfPath, FileMode.Create))
            {
                await pdf.CopyToAsync(stream);
            }
            pdfUrl = $"/uploads/books/{pdfName}";
        }

        // 2. Process Cover (Optional)
        string? coverUrl = null;
        if (cover != null && cover.Length > 0)
        {
            var coverExt = Path.GetExtension(cover.FileName);
            var coverName = $"{Guid.NewGuid()}{coverExt}";
            var coverFolder = Path.Combine(_env.ContentRootPath, "uploads", "covers");
            if (!Directory.Exists(coverFolder)) Directory.CreateDirectory(coverFolder);
            var coverPath = Path.Combine(coverFolder, coverName);

            using (var stream = new FileStream(coverPath, FileMode.Create))
            {
                await cover.CopyToAsync(stream);
            }
            coverUrl = $"/uploads/covers/{coverName}";
        }

        // 3. Save Metadata
        var book = new Book
        {
            Title = title,
            Author = author,
            Genre = genre,
            ISBN = isbn,
            TotalPages = totalPages,
            PublicationYear = publicationYear,
            Visibility = isPublic ? BookVisibility.Public : BookVisibility.Private,
            ModerationStatus = isPublic ? ModerationStatus.Pending : ModerationStatus.None,
            PdfUrl = pdfUrl,
            CoverImageUrl = coverUrl
        };

        var createdBook = await _bookService.CreateBookWithFilesAsync(book, UserId, book.PdfUrl, book.CoverImageUrl);
        return CreatedAtAction(nameof(GetById), new { id = createdBook.Id }, createdBook);
    }

    /// <summary>
    /// Submits a request to make a private volume public.
    /// </summary>
    [HttpPost("{id}/recommend")]
    [ProducesResponseType(typeof(BookRecommendation), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecommendForPublic(Guid id)
    {
        try
        {
            var recommendation = await _bookService.CreateRecommendationAsync(id, UserId);
            return Ok(recommendation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
