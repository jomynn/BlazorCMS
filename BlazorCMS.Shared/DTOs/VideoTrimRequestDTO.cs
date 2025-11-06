namespace BlazorCMS.Shared.DTOs;

public class VideoTrimRequestDTO
{
    public IFormFile? File { get; set; }
    public string? SourceUrl { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string OutputFormat { get; set; } = "mp4";
}
