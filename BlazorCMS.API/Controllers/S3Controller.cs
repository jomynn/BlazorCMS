using BlazorCMS.API.Services;
using BlazorCMS.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BlazorCMS.API.Controllers;

[Route("api/v1/s3")]
[ApiController]
public class S3Controller : ControllerBase
{
    private readonly IS3StorageService _s3Service;
    private readonly ILogger<S3Controller> _logger;

    public S3Controller(IS3StorageService s3Service, ILogger<S3Controller> logger)
    {
        _s3Service = s3Service;
        _logger = logger;
    }

    /// <summary>
    /// Upload file to Amazon S3 storage
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadToS3([FromForm] S3UploadRequestDTO request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.BucketName))
            {
                return BadRequest("BucketName is required");
            }

            if (request.File == null && string.IsNullOrEmpty(request.SourceUrl))
            {
                return BadRequest("Either File or SourceUrl must be provided");
            }

            string result;
            var objectKey = request.ObjectKey ?? Guid.NewGuid().ToString();

            if (request.File != null)
            {
                using var stream = request.File.OpenReadStream();
                objectKey = request.ObjectKey ?? request.File.FileName;
                result = await _s3Service.UploadFileAsync(stream, request.BucketName, objectKey, request.MakePublic);
            }
            else
            {
                result = await _s3Service.UploadFromUrlAsync(request.SourceUrl!, request.BucketName, objectKey, request.MakePublic);
            }

            return Ok(new { url = result, message = "File uploaded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading to S3");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Download file from S3
    /// </summary>
    [HttpGet("download")]
    public async Task<IActionResult> DownloadFromS3([FromQuery] string bucketName, [FromQuery] string objectKey)
    {
        try
        {
            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(objectKey))
            {
                return BadRequest("BucketName and ObjectKey are required");
            }

            var stream = await _s3Service.DownloadFileAsync(bucketName, objectKey);
            return File(stream, "application/octet-stream", objectKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading from S3");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete file from S3
    /// </summary>
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteFromS3([FromQuery] string bucketName, [FromQuery] string objectKey)
    {
        try
        {
            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(objectKey))
            {
                return BadRequest("BucketName and ObjectKey are required");
            }

            var success = await _s3Service.DeleteFileAsync(bucketName, objectKey);

            if (success)
            {
                return Ok(new { message = "File deleted successfully" });
            }
            else
            {
                return StatusCode(500, new { error = "Failed to delete file" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting from S3");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
