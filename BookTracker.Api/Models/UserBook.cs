using System.ComponentModel.DataAnnotations;

namespace BookTracker.Api.Models;

public class UserBook
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    [Required]
    public Guid BookId { get; set; }
    public Book? Book { get; set; }
    
    public ReadingStatus Status { get; set; } = ReadingStatus.WantToRead;
    public int CurrentPage { get; set; } = 0;
    public int? Rating { get; set; }
    public string? Review { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
