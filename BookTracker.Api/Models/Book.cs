using System.ComponentModel.DataAnnotations;

namespace BookTracker.Api.Models;

public class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Author { get; set; } = string.Empty;
    
    [RegularExpression(@"^(?=(?:\D*\d){10}(?:(?:\D*\d){3})?$)[\d-]+$")]
    public string? ISBN { get; set; }
    
    public int? TotalPages { get; set; }
    
    public string? Genre { get; set; }
    
    [Range(1450, 2100)]
    public int? PublicationYear { get; set; }
    
    public string? CoverImageUrl { get; set; }
    
    public ReadingStatus Status { get; set; } = ReadingStatus.WantToRead;
    
    public int CurrentPage { get; set; } = 0;
    
    public int? Rating { get; set; } // 1-5
    
    public string? Review { get; set; }
    
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid UserId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public User? User { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum ReadingStatus
{
    WantToRead,
    Reading,
    Read,
    Dropped
}
