using BookTracker.Api.Models;
using BookTracker.Api.Models.Dtos;

namespace BookTracker.Api.Services;

public interface IBookService
{
    Task<(List<Book> Items, int TotalCount)> GetMyCollectionAsync(Guid userId, string? query, ReadingStatus? status, string? sortBy, int pageNumber = 1, int pageSize = 20);
    Task<(List<Book> Items, int TotalCount)> GetPublicLibraryAsync(string? query, string? genre, string? sortBy, int pageNumber = 1, int pageSize = 20);
    Task<(List<Book> Items, int TotalCount)> GetCompletedBooksAsync(Guid userId, string? query, string? sortBy, int pageNumber = 1, int pageSize = 20);
    Task<Book?> GetBookByIdAsync(Guid id, Guid userId, string userRole);
    Task<Book> CreateBookAsync(BookCreateDto bookDto, Guid userId);
    Task<Book> CreateBookWithFilesAsync(BookCreateDto bookDto, Guid userId, string? pdfUrl, string? coverUrl);
    Task<Book?> UpdateBookAsync(Guid id, BookUpdateDto updateDto, Guid userId, string userRole);
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
