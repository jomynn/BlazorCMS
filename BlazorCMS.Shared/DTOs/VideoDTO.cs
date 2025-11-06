namespace BlazorCMS.Shared.DTOs;

public class VideoDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string? ThumbnailFileName { get; set; }

    // Video Metadata
    public double DurationSeconds { get; set; }
    public string? VideoCodec { get; set; }
    public string? AudioCodec { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSizeBytes { get; set; }
    public string? FrameRate { get; set; }
    public long BitRate { get; set; }

    // Processed versions
    public Dictionary<string, string>? ProcessedVersions { get; set; }

    // Publishing
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedDate { get; set; }

    // Processing
    public string ProcessingStatus { get; set; } = "Pending";
    public string? ProcessingError { get; set; }

    // Author
    public string? AuthorId { get; set; }
    public string? Author { get; set; }

    // Additional
    public string? Tags { get; set; }
    public int ViewCount { get; set; }
}

public class CreateVideoDTO
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public bool IsPublished { get; set; }
}

public class UpdateVideoDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public bool IsPublished { get; set; }
}

public class VideoUploadResponseDTO
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public VideoDTO? Video { get; set; }
}

public class VideoProcessingOptions
{
    public bool GenerateThumbnail { get; set; } = true;
    public List<string> OutputQualities { get; set; } = new() { "1080p", "720p", "480p" };
    public bool ExtractAudio { get; set; } = false;
    public string? WatermarkText { get; set; }
}
