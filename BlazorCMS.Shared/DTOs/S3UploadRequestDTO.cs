namespace BlazorCMS.Shared.DTOs;

public class S3UploadRequestDTO
{
    public IFormFile? File { get; set; }
    public string? SourceUrl { get; set; }
    public string BucketName { get; set; } = string.Empty;
    public string? ObjectKey { get; set; }
    public bool MakePublic { get; set; } = false;
}
