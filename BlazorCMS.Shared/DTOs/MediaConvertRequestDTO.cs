namespace BlazorCMS.Shared.DTOs;

public class MediaConvertRequestDTO
{
    public IFormFile? File { get; set; }
    public string? SourceUrl { get; set; }
    public string OutputFormat { get; set; } = string.Empty;
    public string? Quality { get; set; }
    public int? Bitrate { get; set; }
}
