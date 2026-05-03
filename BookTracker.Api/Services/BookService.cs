using BookTracker.Api.Data;
using BookTracker.Api.Models;
using BookTracker.Api.Constants;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace BookTracker.Api.Services;

public class BookService : IBookService
{
    private readonly AppDbContext _context;

    public BookService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Book>> GetBooksAsync(Guid userId, string? query, ReadingStatus? status, string? genre, int? rating, string? sortBy)
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
        if (!string.IsNullOrWhiteSpace(genre)) queryable = queryable.Where(b => b.Genre == genre);
        if (rating.HasValue) queryable = queryable.Where(b => b.Rating == rating.Value);

        queryable = sortBy?.ToLower() switch
        {
            ApiConstants.SortFields.Title => queryable.OrderBy(b => b.Title),
            ApiConstants.SortFields.Author => queryable.OrderBy(b => b.Author),
            ApiConstants.SortFields.Rating => queryable.OrderByDescending(b => b.Rating),
            _ => queryable.OrderByDescending(b => b.CreatedAt)
        };

        return await queryable.ToListAsync();
    }

    public async Task<Book?> GetBookByIdAsync(Guid id, Guid userId)
    {
        return await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
    }

    public async Task<Book> CreateBookAsync(Book book, Guid userId)
    {
        if (!string.IsNullOrEmpty(book.ISBN))
        {
            if (await _context.Books.AnyAsync(b => b.ISBN == book.ISBN && b.UserId == userId))
                throw new InvalidOperationException("DuplicateISBN");
        }

        book.UserId = userId;
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> UpdateBookAsync(Guid id, Book update, Guid userId)
    {
        var book = await GetBookByIdAsync(id, userId);
        if (book == null) return null;

        if (!string.IsNullOrEmpty(update.ISBN) && update.ISBN != book.ISBN)
        {
            if (await _context.Books.AnyAsync(b => b.ISBN == update.ISBN && b.UserId == userId))
                throw new InvalidOperationException("DuplicateISBN");
        }

        book.Title = update.Title;
        book.Author = update.Author;
        book.ISBN = update.ISBN;
        book.Genre = update.Genre;
        book.TotalPages = update.TotalPages;
        book.PublicationYear = update.PublicationYear;
        book.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<bool> DeleteBookAsync(Guid id, Guid userId)
    {
        var book = await GetBookByIdAsync(id, userId);
        if (book == null) return false;

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Book?> UpdateStatusAsync(Guid id, ReadingStatus status, Guid userId)
    {
        var book = await GetBookByIdAsync(id, userId);
        if (book == null) return null;

        book.Status = status;
        book.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> UpdateProgressAsync(Guid id, int progress, Guid userId)
    {
        var book = await GetBookByIdAsync(id, userId);
        if (book == null) return null;

        book.CurrentPage = progress;
        if (book.TotalPages.HasValue && progress == book.TotalPages.Value)
            book.Status = ReadingStatus.Read;

        book.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> UpdateRatingAsync(Guid id, int rating, Guid userId)
    {
        var book = await GetBookByIdAsync(id, userId);
        if (book == null) return null;

        book.Rating = rating;
        book.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> UpdateCoverAsync(Guid id, string url, Guid userId)
    {
        var book = await GetBookByIdAsync(id, userId);
        if (book == null) return null;

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
}
