using BookTracker.Api.Data;
using BookTracker.Api.Models;
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

    public async Task<List<Book>> GetMyCollectionAsync(Guid userId, string? query, ReadingStatus? status, string? sortBy)
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

        queryable = sortBy?.ToLower() switch
        {
            ApiConstants.SortFields.Title => queryable.OrderBy(b => b.Title),
            ApiConstants.SortFields.Author => queryable.OrderBy(b => b.Author),
            ApiConstants.SortFields.Rating => queryable.OrderByDescending(b => b.Rating),
            _ => queryable.OrderByDescending(b => b.CreatedAt)
        };

        return await queryable.ToListAsync();
    }

    public async Task<List<Book>> GetPublicLibraryAsync(string? query, string? genre, string? sortBy)
    {
        // Public Library = ONLY Public + Approved books (Global)
        var queryable = _context.Books.Where(b => b.Visibility == BookVisibility.Public && b.IsApproved);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLower();
            queryable = queryable.Where(b => 
                (b.Title != null && b.Title.ToLower().Contains(lowerQuery)) || 
                (b.Author != null && b.Author.ToLower().Contains(lowerQuery)));
        }

        if (!string.IsNullOrWhiteSpace(genre)) queryable = queryable.Where(b => b.Genre == genre);

        queryable = sortBy?.ToLower() switch
        {
            ApiConstants.SortFields.Title => queryable.OrderBy(b => b.Title),
            ApiConstants.SortFields.Author => queryable.OrderBy(b => b.Author),
            _ => queryable.OrderByDescending(b => b.CreatedAt)
        };

        var books = await queryable.ToListAsync();

        // Ensure state-neutrality: Public library returns pure bibliographic data only
        foreach (var book in books)
        {
            book.Status = ReadingStatus.WantToRead;
            book.CurrentPage = 0;
            book.Rating = null;
            book.Review = null;
        }

        return books;
    }

    public async Task<List<Book>> GetCompletedBooksAsync(Guid userId, string? query, string? sortBy)
    {
        // Completed Index = Only user's books with status READ
        var queryable = _context.Books.Where(b => b.UserId == userId && b.Status == ReadingStatus.Read);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLower();
            queryable = queryable.Where(b => 
                (b.Title != null && b.Title.ToLower().Contains(lowerQuery)) || 
                (b.Author != null && b.Author.ToLower().Contains(lowerQuery)));
        }

        queryable = sortBy?.ToLower() switch
        {
            ApiConstants.SortFields.Title => queryable.OrderBy(b => b.Title),
            ApiConstants.SortFields.Author => queryable.OrderBy(b => b.Author),
            _ => queryable.OrderByDescending(b => b.UpdatedAt)
        };

        return await queryable.ToListAsync();
    }

    public async Task<Book?> GetBookByIdAsync(Guid id, Guid userId, string userRole)
    {
        IQueryable<Book> query = _context.Books;

        var book = await query.FirstOrDefaultAsync(b => b.Id == id && 
            (b.UserId == userId || (b.Visibility == BookVisibility.Public && b.IsApproved)));

        if (book != null && book.UserId != userId && userRole != "Admin")
        {
            // Data Isolation: Viewer is not owner, return empty reading state
            book.Status = ReadingStatus.WantToRead;
            book.CurrentPage = 0;
            book.Rating = null;
            book.Review = null;
        }

        return book;
    }

    public async Task<Book> CreateBookAsync(Book book, Guid userId)
    {
        if (!string.IsNullOrEmpty(book.ISBN))
        {
            if (await _context.Books.AnyAsync(b => b.ISBN == book.ISBN && b.UserId == userId))
                throw new InvalidOperationException("DuplicateISBN");
        }

        var user = await _context.Users.FindAsync(userId);
        book.UserId = userId;
        book.OwnerRole = user?.Role ?? "User";
        
        if (!string.Equals(book.OwnerRole, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            var requestedPublic = book.ModerationStatus == ModerationStatus.Pending || book.Visibility == BookVisibility.Public;
            book.Visibility = BookVisibility.Private;
            book.IsApproved = false;
            book.ModerationStatus = ModerationStatus.None;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            if (requestedPublic)
            {
                var rec = new BookRecommendation { BookId = book.Id, UserId = userId, Status = ModerationStatus.Pending };
                book.ModerationStatus = ModerationStatus.Pending;
                _context.BookRecommendations.Add(rec);
                await _context.SaveChangesAsync();
            }
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

    public async Task<Book?> UpdateBookAsync(Guid id, Book update, Guid userId, string userRole)
    {
        var book = await GetBookByIdAsync(id, userId, userRole);
        if (book == null) return null;

        // Surgical Update to prevent data loss if partial objects are sent via PUT
        if (!string.IsNullOrEmpty(update.Title)) book.Title = update.Title;
        if (!string.IsNullOrEmpty(update.Author)) book.Author = update.Author;
        
        if (update.ISBN != null) book.ISBN = update.ISBN;
        if (update.Genre != null) book.Genre = update.Genre;
        if (update.TotalPages.HasValue) book.TotalPages = update.TotalPages;
        if (update.PublicationYear.HasValue) book.PublicationYear = update.PublicationYear;
        if (update.Review != null) book.Review = update.Review;
        if (update.Rating.HasValue) book.Rating = update.Rating;

        book.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> PatchBookAsync(Guid id, System.Text.Json.JsonElement update, Guid userId, string userRole)
    {
        var book = await GetBookByIdAsync(id, userId, userRole);
        if (book == null) return null;

        if (update.TryGetProperty("title", out var titleProp) && titleProp.ValueKind != System.Text.Json.JsonValueKind.Null) 
            book.Title = titleProp.GetString() ?? book.Title;
            
        if (update.TryGetProperty("author", out var authorProp) && authorProp.ValueKind != System.Text.Json.JsonValueKind.Null) 
            book.Author = authorProp.GetString() ?? book.Author;

        if (update.TryGetProperty("isbn", out var isbnProp)) 
            book.ISBN = isbnProp.ValueKind == System.Text.Json.JsonValueKind.Null ? null : isbnProp.GetString();

        if (update.TryGetProperty("genre", out var genreProp)) 
            book.Genre = genreProp.ValueKind == System.Text.Json.JsonValueKind.Null ? null : genreProp.GetString();

        if (update.TryGetProperty("totalPages", out var pagesProp)) 
            book.TotalPages = pagesProp.ValueKind == System.Text.Json.JsonValueKind.Null ? null : pagesProp.GetInt32();

        if (update.TryGetProperty("publicationYear", out var yearProp)) 
            book.PublicationYear = yearProp.ValueKind == System.Text.Json.JsonValueKind.Null ? null : yearProp.GetInt32();

        if (update.TryGetProperty("review", out var reviewProp)) 
            book.Review = reviewProp.ValueKind == System.Text.Json.JsonValueKind.Null ? null : reviewProp.GetString();

        if (update.TryGetProperty("rating", out var ratingProp)) 
            book.Rating = ratingProp.ValueKind == System.Text.Json.JsonValueKind.Null ? null : ratingProp.GetInt32();

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

        // Only owner can update reading status
        if (book.UserId != userId && userRole != "Admin") return null;

        book.Status = status;
        book.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> UpdateProgressAsync(Guid id, int progress, Guid userId, string userRole)
    {
        var book = await GetBookByIdAsync(id, userId, userRole);
        if (book == null) {
            _logger.LogWarning("UpdateProgress: Volume {BookId} not found.", id);
            return null;
        }
        if (book.UserId != userId && userRole != "Admin") {
            _logger.LogWarning("UpdateProgress: Unauthorized attempt on volume {BookId} by user {UserId}.", id, userId);
            return null;
        }

        if (book.Status != ReadingStatus.Reading && userRole != "Admin") {
            _logger.LogWarning("UpdateProgress: Attempted progress update on volume {BookId} with status {Status}.", id, book.Status);
            return null;
        }

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
        var books = await _context.Books.Where(b => b.UserId == userId).ToListAsync();
        var currentYear = DateTime.UtcNow.Year;
        var goal = await _context.ReadingGoals.FirstOrDefaultAsync(g => g.UserId == userId && g.TargetYear == currentYear);
        
        return new
        {
            totalBooks = books.Count,
            readBooks = books.Count(b => b.Status == ReadingStatus.Read),
            booksReading = books.Count(b => b.Status == ReadingStatus.Reading),
            booksReadThisYear = books.Count(b => b.Status == ReadingStatus.Read && b.UpdatedAt.Year == currentYear),
            totalPagesRead = books.Sum(b => b.CurrentPage),
            yearlyGoal = goal?.GoalCount ?? 0,
            genreDistribution = books.GroupBy(b => b.Genre ?? "Uncategorized")
                                     .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<byte[]> ExportCsvAsync(Guid userId)
    {
        var books = await _context.Books.Where(b => b.UserId == userId).ToListAsync();
        
        var csv = new StringBuilder();
        csv.AppendLine("Title,Author,ISBN,Genre,Status,CurrentPage,TotalPages,Rating");

        foreach (var book in books)
        {
            csv.AppendLine($"{book.Title},{book.Author},{book.ISBN},{book.Genre},{book.Status},{book.CurrentPage},{book.TotalPages},{book.Rating}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
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

    public async Task<Book> CreateBookWithFilesAsync(Book book, Guid userId, string? pdfUrl, string? coverUrl)
    {
        var user = await _context.Users.FindAsync(userId);
        book.UserId = userId;
        book.OwnerRole = user?.Role ?? "User";
        book.PdfUrl = pdfUrl;
        book.CoverImageUrl = coverUrl;

        if (!string.Equals(book.OwnerRole, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            var requestedPublic = book.ModerationStatus == ModerationStatus.Pending || book.Visibility == BookVisibility.Public;
            book.Visibility = BookVisibility.Private;
            book.IsApproved = false;
            book.ModerationStatus = ModerationStatus.None;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            if (requestedPublic)
            {
                var rec = new BookRecommendation { BookId = book.Id, UserId = userId, Status = ModerationStatus.Pending };
                book.ModerationStatus = ModerationStatus.Pending;
                _context.BookRecommendations.Add(rec);
                await _context.SaveChangesAsync();
            }
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

    // Recommendation Implementation
    public async Task<BookRecommendation> CreateRecommendationAsync(Guid bookId, Guid userId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null || book.UserId != userId) 
            throw new InvalidOperationException("Archival record not found or access denied.");

        // Check for existing pending recommendation
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
