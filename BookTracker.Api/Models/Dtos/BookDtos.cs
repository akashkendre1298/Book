using System.ComponentModel.DataAnnotations;

namespace BookTracker.Api.Models.Dtos;

public class BookDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? ISBN { get; set; }
    public int? TotalPages { get; set; }
    public string? Genre { get; set; }
    public int? PublicationYear { get; set; }
    public string? CoverImageUrl { get; set; }
    public ReadingStatus Status { get; set; }
    public int CurrentPage { get; set; }
    public int? Rating { get; set; }
    public string? Review { get; set; }
    public BookVisibility Visibility { get; set; }
    public ModerationStatus ModerationStatus { get; set; }
    public bool IsApproved { get; set; }
    public string? PdfUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class BookCreateDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Author { get; set; } = string.Empty;
    
    [RegularExpression(@"^(?=(?:\D*\d){10}(?:(?:\D*\d){3})?$)[\d-]+$", ErrorMessage = "Invalid ISBN format")]
    public string? ISBN { get; set; }
    
    public int? TotalPages { get; set; }
    public string? Genre { get; set; }
    
    [Range(1450, 2100)]
    public int? PublicationYear { get; set; }
    
    // Note: IsApproved, Visibility, ModerationStatus are EXCLUDED here to prevent over-posting
}

public class BookUpdateDto
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    
    [RegularExpression(@"^(?=(?:\D*\d){10}(?:(?:\D*\d){3})?$)[\d-]+$", ErrorMessage = "Invalid ISBN format")]
    public string? ISBN { get; set; }
    
    public int? TotalPages { get; set; }
    public string? Genre { get; set; }
    
    [Range(1450, 2100)]
    public int? PublicationYear { get; set; }
    
    public string? Review { get; set; }
    public int? Rating { get; set; }
    public ReadingStatus? Status { get; set; }
    public int? CurrentPage { get; set; }
}
