using BookTracker.Api.Data;
using BookTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Api.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;

    public AuditService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string action, string performedBy, string? details = null)
    {
        var log = new AuditLog
        {
            Action = action,
            PerformedBy = performedBy,
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetLogsAsync(int count = 50)
    {
        return await _context.AuditLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .ToListAsync();
    }
}
