using BookTracker.Api.Models;

namespace BookTracker.Api.Services;

public interface IBookService
{
    Task<List<Book>> GetBooksAsync(Guid userId, string? query, ReadingStatus? status, string? genre, int? rating, string? sortBy);
    Task<Book?> GetBookByIdAsync(Guid id, Guid userId);
    Task<Book> CreateBookAsync(Book book, Guid userId);
    Task<Book?> UpdateBookAsync(Guid id, Book update, Guid userId);
    Task<bool> DeleteBookAsync(Guid id, Guid userId);
    Task<Book?> UpdateStatusAsync(Guid id, ReadingStatus status, Guid userId);
    Task<Book?> UpdateProgressAsync(Guid id, int progress, Guid userId);
    Task<Book?> UpdateRatingAsync(Guid id, int rating, Guid userId);
    Task<Book?> UpdateCoverAsync(Guid id, string url, Guid userId);
    Task<object> GetStatsAsync(Guid userId);
    Task<byte[]> ExportCsvAsync(Guid userId);
    Task<ReadingGoal> SetGoalAsync(ReadingGoal goal, Guid userId);
}
