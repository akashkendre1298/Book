using BookTracker.Api.Models;

namespace BookTracker.Api.Services;

public interface IAuditService
{
    Task LogAsync(string action, string performedBy, string? details = null);
    Task<List<AuditLog>> GetLogsAsync(int count = 50);
}
