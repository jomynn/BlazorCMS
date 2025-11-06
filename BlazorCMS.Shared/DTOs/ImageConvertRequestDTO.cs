namespace BlazorCMS.Shared.DTOs;

public class ImageConvertRequestDTO
{
    public IFormFile? File { get; set; }
    public string? SourceUrl { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string OutputFormat { get; set; } = "jpg";
    public int Quality { get; set; } = 90;
    public bool MaintainAspectRatio { get; set; } = true;
}
