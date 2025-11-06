using BlazorCMS.Shared.DTOs;

namespace BlazorCMS.API.Services;

public interface IS3StorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string bucketName, string objectKey, bool makePublic);
    Task<string> UploadFromUrlAsync(string sourceUrl, string bucketName, string objectKey, bool makePublic);
    Task<Stream> DownloadFileAsync(string bucketName, string objectKey);
    Task<bool> DeleteFileAsync(string bucketName, string objectKey);
}
