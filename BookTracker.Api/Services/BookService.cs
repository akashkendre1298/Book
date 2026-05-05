using BookTracker.Api.Data;
using BookTracker.Api.Models;
using BookTracker.Api.Models.Dtos;
using BookTracker.Api.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace BookTracker.Api.Services;

public class BookService : IBookService
{
    private readonly AppDbContext _context;
    private readonly ILogger<BookService> _logger;

    public BookService(AppDbContext context, ILogger<BookService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<Book> Items, int TotalCount)> GetMyCollectionAsync(Guid userId, string? query, ReadingStatus? status, string? sortBy, int pageNumber = 1, int pageSize = 20)
    {
        var queryable = _context.Books.Where(b => b.UserId == userId);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLower();
            queryable = queryable.Where(b => 
                (b.Title != null && b.Title.ToLower().Contains(lowerQuery)) || 
                (b.Author != null && b.Author.ToLower().Contains(lowerQuery)) ||
                (b.ISBN != null && b.ISBN.Contains(lowerQuery)));
        }

        if (status.HasValue) queryable = queryable.Where(b => b.Status == status.Value);

        var totalCount = await queryable.CountAsync();

        queryable = sortBy?.ToLower() switch
        {
            ApiConstants.SortFields.Title => queryable.OrderBy(b => b.Title),
            ApiConstants.SortFields.Author => queryable.OrderBy(b => b.Author),
            ApiConstants.SortFields.Rating => queryable.OrderByDescending(b => b.Rating),
            _ => queryable.OrderByDescending(b => b.CreatedAt)
        };

        var items = await queryable
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<Book> Items, int TotalCount)> GetPublicLibraryAsync(string? query, string? genre, string? sortBy, int pageNumber = 1, int pageSize = 20)
    {
        var queryable = _context.Books.Where(b => b.Visibility == BookVisibility.Public && b.IsApproved);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLower();
            queryable = queryable.Where(b => 
                (b.Title != null && b.Title.ToLower().Contains(lowerQuery)) || 
                (b.Author != null && b.Author.ToLower().Contains(lowerQuery)));
        }

        if (!string.IsNullOrWhiteSpace(genre)) queryable = queryable.Where(b => b.Genre == genre);

        var totalCount = await queryable.CountAsync();

        queryable = sortBy?.ToLower() switch
        {
            ApiConstants.SortFields.Title => queryable.OrderBy(b => b.Title),
            ApiConstants.SortFields.Author => queryable.OrderBy(b => b.Author),
            _ => queryable.OrderByDescending(b => b.CreatedAt)
        };

        var items = await queryable
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Ensure state-neutrality: Public library returns pure bibliographic data only
        foreach (var book in items)
        {
            // Reset to default archival state for the public index
            book.Status = ReadingStatus.WantToRead;
            book.CurrentPage = 0;
            book.Rating = null;
            book.Review = null;
        }

        return (items, totalCount);
    }

    public async Task<(List<Book> Items, int TotalCount)> GetCompletedBooksAsync(Guid userId, string? query, string? sortBy, int pageNumber = 1, int pageSize = 20)
    {
        var queryable = _context.Books.Where(b => b.UserId == userId && b.Status == ReadingStatus.Read);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLower();
            queryable = queryable.Where(b => 
                (b.Title != null && b.Title.ToLower().Contains(lowerQuery)) || 
                (b.Author != null && b.Author.ToLower().Contains(lowerQuery)));
        }

        var totalCount = await queryable.CountAsync();

        queryable = sortBy?.ToLower() switch
        {
            ApiConstants.SortFields.Title => queryable.OrderBy(b => b.Title),
            ApiConstants.SortFields.Author => queryable.OrderBy(b => b.Author),
            _ => queryable.OrderByDescending(b => b.UpdatedAt)
        };

        var items = await queryable
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Book?> GetBookByIdAsync(Guid id, Guid userId, string userRole)
    {
        var book = await _context.Books.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id);
        
        if (book == null) return null;

        // Authorization: Owner or Public/Approved or Admin
        bool isOwner = book.UserId == userId;
        bool isPublic = book.Visibility == BookVisibility.Public && book.IsApproved;
        bool isAdmin = string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase);

        if (!isOwner && !isPublic && !isAdmin) return null;

        // Strip personal data if requester is not the owner (Bibliographic Neutrality)
        if (!isOwner && !isAdmin)
        {
            book.Status = ReadingStatus.WantToRead;
            book.CurrentPage = 0;
            book.Rating = null;
            book.Review = null;
        }

        return book;
    }

    public async Task<Book> CreateBookAsync(BookCreateDto bookDto, Guid userId)
    {
        if (!string.IsNullOrEmpty(bookDto.ISBN))
        {
            if (await _context.Books.AnyAsync(b => b.ISBN == bookDto.ISBN && b.UserId == userId))
                throw new InvalidOperationException("DuplicateISBN");
        }

        var user = await _context.Users.FindAsync(userId);
        
        var book = new Book
        {
            Title = bookDto.Title,
            Author = bookDto.Author,
            ISBN = bookDto.ISBN,
            Genre = bookDto.Genre,
            TotalPages = bookDto.TotalPages,
            PublicationYear = bookDto.PublicationYear,
            UserId = userId,
            OwnerRole = user?.Role ?? "User"
        };
        
        if (!string.Equals(book.OwnerRole, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            book.Visibility = BookVisibility.Private;
            book.IsApproved = false;
            book.ModerationStatus = ModerationStatus.None;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }
        else
        {
            book.Visibility = BookVisibility.Public;
            book.IsApproved = true;
            book.ModerationStatus = ModerationStatus.Approved;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }

        return book;
    }

    public async Task<Book?> UpdateBookAsync(Guid id, BookUpdateDto updateDto, Guid userId, string userRole)
    {
        var book = await GetBookByIdAsync(id, userId, userRole);
        if (book == null) return null;

        // Surgical Update to prevent over-posting and ensure data integrity
        if (!string.IsNullOrEmpty(updateDto.Title)) book.Title = updateDto.Title;
        if (!string.IsNullOrEmpty(updateDto.Author)) book.Author = updateDto.Author;
        
        if (updateDto.ISBN != null) book.ISBN = updateDto.ISBN;
        if (updateDto.Genre != null) book.Genre = updateDto.Genre;
        if (updateDto.TotalPages.HasValue) book.TotalPages = updateDto.TotalPages;
        if (updateDto.PublicationYear.HasValue) book.PublicationYear = updateDto.PublicationYear;
        if (updateDto.Review != null) book.Review = updateDto.Review;
        if (updateDto.Rating.HasValue) book.Rating = updateDto.Rating;
        if (updateDto.Status.HasValue) book.Status = updateDto.Status.Value;
        if (updateDto.CurrentPage.HasValue) book.CurrentPage = updateDto.CurrentPage.Value;

        book.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> PatchBookAsync(Guid id, JsonElement update, Guid userId, string userRole)
    {
        var book = await GetBookByIdAsync(id, userId, userRole);
        if (book == null) return null;

        // Explicit property mapping to prevent mass assignment of internal fields
        if (update.TryGetProperty("title", out var titleProp) && titleProp.ValueKind != JsonValueKind.Null) 
            book.Title = titleProp.GetString() ?? book.Title;
            
        if (update.TryGetProperty("author", out var authorProp) && authorProp.ValueKind != JsonValueKind.Null) 
            book.Author = authorProp.GetString() ?? book.Author;

        if (update.TryGetProperty("isbn", out var isbnProp)) 
            book.ISBN = isbnProp.ValueKind == JsonValueKind.Null ? null : isbnProp.GetString();

        if (update.TryGetProperty("genre", out var genreProp)) 
            book.Genre = genreProp.ValueKind == JsonValueKind.Null ? null : genreProp.GetString();

        if (update.TryGetProperty("totalPages", out var pagesProp)) 
            book.TotalPages = pagesProp.ValueKind == JsonValueKind.Null ? null : pagesProp.GetInt32();

        if (update.TryGetProperty("publicationYear", out var yearProp)) 
            book.PublicationYear = yearProp.ValueKind == JsonValueKind.Null ? null : yearProp.GetInt32();

        if (update.TryGetProperty("review", out var reviewProp)) 
            book.Review = reviewProp.ValueKind == JsonValueKind.Null ? null : reviewProp.GetString();

        if (update.TryGetProperty("rating", out var ratingProp)) 
            book.Rating = ratingProp.ValueKind == JsonValueKind.Null ? null : ratingProp.GetInt32();

        if (update.TryGetProperty("status", out var statusProp))
            book.Status = (ReadingStatus)statusProp.GetInt32();

        if (update.TryGetProperty("currentPage", out var currentProp))
            book.CurrentPage = currentProp.GetInt32();

        book.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<bool> DeleteBookAsync(Guid id, Guid userId, string userRole)
    {
        var book = await GetBookByIdAsync(id, userId, userRole);
        if (book == null) return false;

        // Cleanup recommendations if any
        var recs = _context.BookRecommendations.Where(r => r.BookId == id);
        _context.BookRecommendations.RemoveRange(recs);

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Book?> UpdateStatusAsync(Guid id, ReadingStatus status, Guid userId, string userRole)
    {
        var book = await GetBookByIdAsync(id, userId, userRole);
        if (book == null) return null;

        if (book.UserId != userId && userRole != "Admin") return null;

        book.Status = status;
        book.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> UpdateProgressAsync(Guid id, int progress, Guid userId, string userRole)
    {
        var book = await GetBookByIdAsync(id, userId, userRole);
        if (book == null) return null;
        
        if (book.UserId != userId && userRole != "Admin") return null;

        if (book.Status != ReadingStatus.Reading && userRole != "Admin") return null;

        book.CurrentPage = progress;
        if (book.TotalPages.HasValue && progress == book.TotalPages.Value)
            book.Status = ReadingStatus.Read;

        book.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> UpdateRatingAsync(Guid id, int rating, Guid userId, string userRole)
    {
        var book = await GetBookByIdAsync(id, userId, userRole);
        if (book == null) return null;
        if (book.UserId != userId && userRole != "Admin") return null;

        book.Rating = rating;
        book.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> UpdateCoverAsync(Guid id, string url, Guid userId, string userRole)
    {
        var book = await GetBookByIdAsync(id, userId, userRole);
        if (book == null) return null;
        if (book.UserId != userId && userRole != "Admin") return null;

        book.CoverImageUrl = url;
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<object> GetStatsAsync(Guid userId)
    {
        // Optimized with async aggregations on the DB side
        var currentYear = DateTime.UtcNow.Year;
        
        var totalBooks = await _context.Books.CountAsync(b => b.UserId == userId);
        var readBooks = await _context.Books.CountAsync(b => b.UserId == userId && b.Status == ReadingStatus.Read);
        var booksReading = await _context.Books.CountAsync(b => b.UserId == userId && b.Status == ReadingStatus.Reading);
        var booksReadThisYear = await _context.Books.CountAsync(b => b.UserId == userId && b.Status == ReadingStatus.Read && b.UpdatedAt.Year == currentYear);
        var totalPagesRead = await _context.Books.Where(b => b.UserId == userId).SumAsync(b => b.CurrentPage);
        
        var goal = await _context.ReadingGoals.FirstOrDefaultAsync(g => g.UserId == userId && g.TargetYear == currentYear);
        
        var genreDistribution = await _context.Books
            .Where(b => b.UserId == userId)
            .GroupBy(b => b.Genre ?? "Uncategorized")
            .Select(g => new { Genre = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Genre, x => x.Count);
        
        return new
        {
            totalBooks,
            readBooks,
            booksReading,
            booksReadThisYear,
            totalPagesRead,
            yearlyGoal = goal?.GoalCount ?? 0,
            genreDistribution
        };
    }

    public async Task<byte[]> ExportCsvAsync(Guid userId)
    {
        var books = await _context.Books.Where(b => b.UserId == userId).ToListAsync();
        
        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
        using (var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
        {
            // CsvHelper handles escaping automatically to prevent CSV injection
            await csv.WriteRecordsAsync(books.Select(b => new {
                b.Title,
                b.Author,
                b.ISBN,
                b.Genre,
                Status = b.Status.ToString(),
                b.CurrentPage,
                b.TotalPages,
                b.Rating
            }));
        }

        return memoryStream.ToArray();
    }

    public async Task<ReadingGoal> SetGoalAsync(ReadingGoal goal, Guid userId)
    {
        var existing = await _context.ReadingGoals.FirstOrDefaultAsync(g => g.UserId == userId && g.TargetYear == goal.TargetYear);
        if (existing != null)
        {
            existing.GoalCount = goal.GoalCount;
            await _context.SaveChangesAsync();
            return existing;
        }
        
        goal.UserId = userId;
        _context.ReadingGoals.Add(goal);
        await _context.SaveChangesAsync();
        return goal;
    }

    public async Task<Book> CreateBookWithFilesAsync(BookCreateDto bookDto, Guid userId, string? pdfUrl, string? coverUrl)
    {
        var user = await _context.Users.FindAsync(userId);
        
        var book = new Book
        {
            Title = bookDto.Title,
            Author = bookDto.Author,
            ISBN = bookDto.ISBN,
            Genre = bookDto.Genre,
            TotalPages = bookDto.TotalPages,
            PublicationYear = bookDto.PublicationYear,
            UserId = userId,
            OwnerRole = user?.Role ?? "User",
            PdfUrl = pdfUrl,
            CoverImageUrl = coverUrl
        };

        if (!string.Equals(book.OwnerRole, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            book.Visibility = BookVisibility.Private;
            book.IsApproved = false;
            book.ModerationStatus = ModerationStatus.None;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }
        else
        {
            book.Visibility = BookVisibility.Public;
            book.IsApproved = true;
            book.ModerationStatus = ModerationStatus.Approved;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }

        return book;
    }

    public async Task<BookRecommendation> CreateRecommendationAsync(Guid bookId, Guid userId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null || book.UserId != userId) 
            throw new InvalidOperationException("Archival record not found or access denied.");

        var existing = await _context.BookRecommendations.FirstOrDefaultAsync(r => r.BookId == bookId && r.Status == ModerationStatus.Pending);
        if (existing != null) return existing;

        var recommendation = new BookRecommendation
        {
            BookId = bookId,
            UserId = userId,
            Status = ModerationStatus.Pending
        };

        book.ModerationStatus = ModerationStatus.Pending;
        
        _context.BookRecommendations.Add(recommendation);
        await _context.SaveChangesAsync();
        return recommendation;
    }

    public async Task<List<BookRecommendation>> GetPendingRecommendationsAsync()
    {
        return await _context.BookRecommendations
            .Include(r => r.Book)
            .Include(r => r.User)
            .Where(r => r.Status == ModerationStatus.Pending)
            .OrderBy(r => r.RequestedAt)
            .ToListAsync();
    }

    public async Task<bool> HandleRecommendationAsync(Guid recommendationId, bool approve, string? adminNotes)
    {
        var rec = await _context.BookRecommendations.Include(r => r.Book).FirstOrDefaultAsync(r => r.Id == recommendationId);
        if (rec == null) return false;

        rec.Status = approve ? ModerationStatus.Approved : ModerationStatus.Rejected;
        rec.AdminNotes = adminNotes;
        rec.ReviewedAt = DateTime.UtcNow;

        if (rec.Book != null)
        {
            rec.Book.ModerationStatus = rec.Status;
            rec.Book.IsApproved = approve;
            rec.Book.Visibility = approve ? BookVisibility.Public : BookVisibility.Private;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
