namespace BookTracker.Api.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subFolder, string[] allowedExtensions, long maxSizeBytes);
    bool DeleteFile(string relativePath);
    bool IsPdf(IFormFile file);
    bool IsImage(IFormFile file);
}
