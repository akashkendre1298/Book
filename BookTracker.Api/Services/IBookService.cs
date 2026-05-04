using BookTracker.Api.Models;

namespace BookTracker.Api.Services;

public interface IBookService
{
    Task<List<Book>> GetMyCollectionAsync(Guid userId, string? query, ReadingStatus? status, string? sortBy);
    Task<List<Book>> GetPublicLibraryAsync(string? query, string? genre, string? sortBy);
    Task<List<Book>> GetCompletedBooksAsync(Guid userId, string? query, string? sortBy);
    Task<Book?> GetBookByIdAsync(Guid id, Guid userId, string userRole);
    Task<Book> CreateBookAsync(Book book, Guid userId);
    Task<Book> CreateBookWithFilesAsync(Book book, Guid userId, string? pdfUrl, string? coverUrl);
    Task<Book?> UpdateBookAsync(Guid id, Book update, Guid userId, string userRole);
    Task<Book?> PatchBookAsync(Guid id, System.Text.Json.JsonElement update, Guid userId, string userRole);
    Task<bool> DeleteBookAsync(Guid id, Guid userId, string userRole);
    Task<Book?> UpdateStatusAsync(Guid id, ReadingStatus status, Guid userId, string userRole);
    Task<Book?> UpdateProgressAsync(Guid id, int progress, Guid userId, string userRole);
    Task<Book?> UpdateRatingAsync(Guid id, int rating, Guid userId, string userRole);
    Task<Book?> UpdateCoverAsync(Guid id, string url, Guid userId, string userRole);
    Task<object> GetStatsAsync(Guid userId);
    Task<byte[]> ExportCsvAsync(Guid userId);
    Task<ReadingGoal> SetGoalAsync(ReadingGoal goal, Guid userId);
    
    // Recommendations
    Task<BookRecommendation> CreateRecommendationAsync(Guid bookId, Guid userId);
    Task<List<BookRecommendation>> GetPendingRecommendationsAsync();
    Task<bool> HandleRecommendationAsync(Guid recommendationId, bool approve, string? adminNotes);
}
