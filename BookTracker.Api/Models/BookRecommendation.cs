using System.ComponentModel.DataAnnotations;

namespace BookTracker.Api.Models;

public class BookRecommendation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid BookId { get; set; }
    public Book? Book { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    public ModerationStatus Status { get; set; } = ModerationStatus.Pending;
    
    public string? AdminNotes { get; set; }
    
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}
