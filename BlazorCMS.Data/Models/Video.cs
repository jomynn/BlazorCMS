namespace BlazorCMS.Data.Models;

public class Video
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public TimeSpan Duration { get; set; }

    public string? Resolution { get; set; }

    public string? Codec { get; set; }

    public double FrameRate { get; set; }

    public string? AudioCodec { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public string UploadedBy { get; set; } = string.Empty;

    public VideoStatus Status { get; set; } = VideoStatus.Uploaded;

    public string? MergedVideoPath { get; set; }

    public int? MergeGroupId { get; set; }
}

public enum VideoStatus
{
    Uploading,
    Uploaded,
    Processing,
    Processed,
    Merged,
    Failed
}

public class VideoMergeJob
{
    public int Id { get; set; }

    public string JobId { get; set; } = Guid.NewGuid().ToString();

    public List<int> VideoIds { get; set; } = new();

    public string? OutputPath { get; set; }

    public TimeSpan TotalDuration { get; set; }

    public VideoMergeStatus Status { get; set; } = VideoMergeStatus.Pending;

    public int Progress { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}

public enum VideoMergeStatus
{
    Pending,
    InProgress,
    Completed,
    Failed
}
