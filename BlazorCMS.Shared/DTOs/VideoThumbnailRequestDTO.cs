namespace BlazorCMS.Shared.DTOs;

public class VideoThumbnailRequestDTO
{
    public IFormFile? File { get; set; }
    public string? SourceUrl { get; set; }
    public TimeSpan? Timestamp { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}
