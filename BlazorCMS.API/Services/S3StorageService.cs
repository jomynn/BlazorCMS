using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using BlazorCMS.Shared.DTOs;

namespace BlazorCMS.API.Services;

public class S3StorageService : IS3StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3StorageService> _logger;
    private readonly HttpClient _httpClient;

    public S3StorageService(IAmazonS3 s3Client, ILogger<S3StorageService> logger, IHttpClientFactory httpClientFactory)
    {
        _s3Client = s3Client;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string bucketName, string objectKey, bool makePublic)
    {
        try
        {
            var transferUtility = new TransferUtility(_s3Client);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                BucketName = bucketName,
                Key = objectKey,
                CannedACL = makePublic ? S3CannedACL.PublicRead : S3CannedACL.Private
            };

            await transferUtility.UploadAsync(uploadRequest);

            var url = $"https://{bucketName}.s3.amazonaws.com/{objectKey}";
            _logger.LogInformation($"File uploaded successfully to {url}");

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to S3");
            throw;
        }
    }

    public async Task<string> UploadFromUrlAsync(string sourceUrl, string bucketName, string objectKey, bool makePublic)
    {
        try
        {
            // Download from URL
            using var response = await _httpClient.GetAsync(sourceUrl);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();

            // Upload to S3
            return await UploadFileAsync(stream, bucketName, objectKey, makePublic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading from URL to S3");
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string bucketName, string objectKey)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey
            };

            var response = await _s3Client.GetObjectAsync(request);
            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from S3");
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string bucketName, string objectKey)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey
            };

            await _s3Client.DeleteObjectAsync(request);
            _logger.LogInformation($"File deleted successfully: {bucketName}/{objectKey}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from S3");
            return false;
        }
    }
}
