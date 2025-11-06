namespace BlazorCMS.Shared.DTOs;

public class MediaMetadataResponseDTO
{
    public string? FileName { get; set; }
    public long FileSize { get; set; }
    public string? MimeType { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? Format { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? FrameRate { get; set; }
    public int? Bitrate { get; set; }
    public string? AudioCodec { get; set; }
    public string? VideoCodec { get; set; }
    public Dictionary<string, string> AdditionalMetadata { get; set; } = new();
}
