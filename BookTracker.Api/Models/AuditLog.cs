using System.ComponentModel.DataAnnotations;

namespace BookTracker.Api.Models;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Action { get; set; } = string.Empty; // e.g., "UserBlocked", "BookApproved"
    
    [Required]
    public string PerformedBy { get; set; } = string.Empty; // Email of the admin
    
    public string? Details { get; set; } // JSON or descriptive string
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
