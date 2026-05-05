using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookTracker.Api.Models;
using BookTracker.Api.Models.Dtos;
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
    [HttpGet]
    [HttpGet("my")]
    [ProducesResponseType(typeof(PagedResult<Book>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCollection(
        [FromQuery] string? query, 
        [FromQuery] ReadingStatus? status, 
        [FromQuery] string? sortBy = ApiConstants.DefaultSortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var (items, totalCount) = await _bookService.GetMyCollectionAsync(UserId, query, status, sortBy, page, pageSize);
        return Ok(new PagedResult<Book>(items, totalCount, page, pageSize));
    }

    /// <summary>
    /// Retrieves books from the public library (Approved Public volumes).
    /// </summary>
    [HttpGet("public")]
    [ProducesResponseType(typeof(PagedResult<Book>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicLibrary(
        [FromQuery] string? query, 
        [FromQuery] string? genre,
        [FromQuery] string? sortBy = ApiConstants.DefaultSortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var (items, totalCount) = await _bookService.GetPublicLibraryAsync(query, genre, sortBy, page, pageSize);
        return Ok(new PagedResult<Book>(items, totalCount, page, pageSize));
    }

    /// <summary>
    /// Retrieves only the user's completed volumes.
    /// </summary>
    [HttpGet("completed")]
    [ProducesResponseType(typeof(PagedResult<Book>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompleted(
        [FromQuery] string? query, 
        [FromQuery] string? sortBy = ApiConstants.DefaultSortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var (items, totalCount) = await _bookService.GetCompletedBooksAsync(UserId, query, sortBy, page, pageSize);
        return Ok(new PagedResult<Book>(items, totalCount, page, pageSize));
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
    public async Task<IActionResult> Create([FromBody] BookCreateDto bookDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var createdBook = await _bookService.CreateBookAsync(bookDto, UserId);
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
    public async Task<IActionResult> Update(Guid id, [FromBody] BookUpdateDto updateDto)
    {
        try
        {
            var book = await _bookService.UpdateBookAsync(id, updateDto, UserId, UserRole);
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
    public async Task<IActionResult> UploadCover(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded");
        if (file.Length > ApiConstants.MaxCoverImageSize) 
            return BadRequest($"File too large (max {ApiConstants.MaxCoverImageSize / (1024 * 1024)}MB)");
        
        if (!ApiConstants.AllowedCoverImageTypes.Contains(file.ContentType)) 
            return BadRequest("Invalid file type. Allowed: .jpg, .jpeg, .png");
        
        var book = await _bookService.GetBookByIdAsync(id, UserId, UserRole);
        if (book == null) return NotFound();

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        
        var folderPath = Path.Combine(_env.ContentRootPath, "uploads");
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var url = $"/uploads/{fileName}";
        await _bookService.UpdateCoverAsync(id, url, UserId, UserRole);

        return Ok(new { url });
    }

    /// <summary>
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> PatchStatus(Guid id, [FromBody] JsonElement body)
    {
        if (body.TryGetProperty("status", out var statusProp))
        {
            ReadingStatus status;
            if (statusProp.ValueKind == JsonValueKind.String)
            {
                if (!Enum.TryParse(statusProp.GetString(), true, out status))
                    return BadRequest("Invalid status string");
            }
            else
            {
                status = (ReadingStatus)statusProp.GetInt32();
            }

            var book = await _bookService.UpdateStatusAsync(id, status, UserId, UserRole);
            if (book == null) return NotFound();
            return Ok(book);
        }
        return BadRequest();
    }

    /// <summary>
    /// Updates the current reading progress (page number).
    /// </summary>
    [HttpPatch("{id}/progress")]
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
    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var csvBytes = await _bookService.ExportCsvAsync(UserId);
        return File(csvBytes, "text/csv", $"athenaeum_export_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    /// <summary>
    /// Uploads a complete digital volume (Metadata + PDF + Cover).
    /// </summary>
    [HttpPost("upload")]
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
        // Use DTO for metadata validation and mapping
        var bookDto = new BookCreateDto
        {
            Title = title,
            Author = author,
            Genre = genre,
            ISBN = isbn,
            TotalPages = totalPages,
            PublicationYear = publicationYear
        };

        if (!TryValidateModel(bookDto)) return BadRequest(ModelState);

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

        var createdBook = await _bookService.CreateBookWithFilesAsync(bookDto, UserId, pdfUrl, coverUrl);
        return CreatedAtAction(nameof(GetById), new { id = createdBook.Id }, createdBook);
    }

    /// <summary>
    /// Submits a request to make a private volume public.
    /// </summary>
    [HttpPost("{id}/recommend")]
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

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
