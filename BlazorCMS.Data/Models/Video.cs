using System.ComponentModel.DataAnnotations;

namespace BlazorCMS.Data.Models;

public class Video
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(500)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string StoredFileName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ThumbnailFileName { get; set; }

    // Video Metadata
    public double DurationSeconds { get; set; }

    [MaxLength(50)]
    public string? VideoCodec { get; set; }

    [MaxLength(50)]
    public string? AudioCodec { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public long FileSizeBytes { get; set; }

    [MaxLength(20)]
    public string? FrameRate { get; set; }

    public long BitRate { get; set; }

    // Processed Versions (stored as JSON or separate table)
    [MaxLength(2000)]
    public string? ProcessedVersions { get; set; } // JSON: {"720p": "filename.mp4", "480p": "filename_480.mp4"}

    // Publishing
    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PublishedDate { get; set; }

    // Processing Status
    [MaxLength(50)]
    public string ProcessingStatus { get; set; } = "Pending"; // Pending, Processing, Completed, Failed

    [MaxLength(1000)]
    public string? ProcessingError { get; set; }

    // Author/User relationship
    public string? AuthorId { get; set; }

    [MaxLength(100)]
    public string? Author { get; set; }

    // Categories/Tags
    [MaxLength(500)]
    public string? Tags { get; set; } // Comma-separated or JSON

    // View tracking
    public int ViewCount { get; set; }
}
