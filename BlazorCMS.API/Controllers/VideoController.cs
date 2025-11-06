using BlazorCMS.API.Services;
using BlazorCMS.Infrastructure.Video;
using BlazorCMS.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlazorCMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoController : ControllerBase
{
    private readonly VideoService _videoService;
    private readonly VideoProcessingService _processingService;
    private readonly ILogger<VideoController> _logger;

    public VideoController(
        VideoService videoService,
        VideoProcessingService processingService,
        ILogger<VideoController> logger)
    {
        _videoService = videoService;
        _processingService = processingService;
        _logger = logger;
    }

    /// <summary>
    /// Get all videos (admin)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<VideoDTO>>> GetAll()
    {
        try
        {
            var videos = await _videoService.GetAllAsync();
            return Ok(videos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all videos");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get published videos (public)
    /// </summary>
    [HttpGet("published")]
    public async Task<ActionResult<List<VideoDTO>>> GetPublished()
    {
        try
        {
            var videos = await _videoService.GetPublishedAsync();
            return Ok(videos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get published videos");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get video by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VideoDTO>> GetById(int id)
    {
        try
        {
            var video = await _videoService.GetByIdAsync(id);
            if (video == null)
                return NotFound($"Video with ID {id} not found");

            return Ok(video);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get video {VideoId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get videos by author
    /// </summary>
    [HttpGet("author/{authorId}")]
    public async Task<ActionResult<List<VideoDTO>>> GetByAuthor(string authorId)
    {
        try
        {
            var videos = await _videoService.GetByAuthorAsync(authorId);
            return Ok(videos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get videos by author {AuthorId}", authorId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Upload and process a new video
    /// </summary>
    [HttpPost("upload")]
    [Authorize]
    public async Task<ActionResult<VideoUploadResponseDTO>> Upload(
        [FromForm] IFormFile file,
        [FromForm] string title,
        [FromForm] string? description,
        [FromForm] string? tags,
        [FromForm] bool isPublished = false)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            var createDto = new CreateVideoDTO
            {
                Title = title,
                Description = description,
                Tags = tags,
                IsPublished = isPublished
            };

            var result = await _videoService.UploadVideoAsync(file, createDto, userId, userName);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload video");
            return StatusCode(500, new VideoUploadResponseDTO
            {
                Success = false,
                Message = $"Upload failed: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Update video metadata
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<VideoDTO>> Update(int id, [FromBody] UpdateVideoDTO updateDto)
    {
        try
        {
            if (id != updateDto.Id)
                return BadRequest("ID mismatch");

            var video = await _videoService.UpdateAsync(updateDto);
            if (video == null)
                return NotFound($"Video with ID {id} not found");

            return Ok(video);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update video {VideoId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete video
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var result = await _videoService.DeleteAsync(id);
            if (!result)
                return NotFound($"Video with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete video {VideoId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search videos
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<VideoDTO>>> Search([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Search term is required");

            var videos = await _videoService.SearchAsync(q);
            return Ok(videos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search videos with term: {SearchTerm}", q);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Stream video file
    /// </summary>
    [HttpGet("{id}/stream")]
    public async Task<IActionResult> Stream(int id, [FromQuery] string? quality = "original")
    {
        try
        {
            var video = await _videoService.GetByIdAsync(id);
            if (video == null)
                return NotFound($"Video with ID {id} not found");

            // Increment view count
            await _videoService.IncrementViewCountAsync(id);

            // Get the requested quality version
            string fileName = video.StoredFileName;

            if (!string.IsNullOrEmpty(quality) && quality != "original" && video.ProcessedVersions != null)
            {
                if (video.ProcessedVersions.TryGetValue(quality, out var qualityFileName))
                {
                    fileName = qualityFileName;
                }
            }

            var filePath = _processingService.GetVideoPath(fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Video file not found");

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var mimeType = GetMimeType(Path.GetExtension(filePath));

            return File(fileStream, mimeType, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stream video {VideoId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get video thumbnail
    /// </summary>
    [HttpGet("{id}/thumbnail")]
    public async Task<IActionResult> GetThumbnail(int id)
    {
        try
        {
            var video = await _videoService.GetByIdAsync(id);
            if (video == null || string.IsNullOrEmpty(video.ThumbnailFileName))
                return NotFound("Thumbnail not found");

            var thumbnailPath = _processingService.GetThumbnailPath(video.ThumbnailFileName);

            if (!System.IO.File.Exists(thumbnailPath))
                return NotFound("Thumbnail file not found");

            var fileStream = new FileStream(thumbnailPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(fileStream, "image/jpeg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get thumbnail for video {VideoId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Download video file
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id, [FromQuery] string? quality = "original")
    {
        try
        {
            var video = await _videoService.GetByIdAsync(id);
            if (video == null)
                return NotFound($"Video with ID {id} not found");

            string fileName = video.StoredFileName;
            string downloadName = video.OriginalFileName;

            if (!string.IsNullOrEmpty(quality) && quality != "original" && video.ProcessedVersions != null)
            {
                if (video.ProcessedVersions.TryGetValue(quality, out var qualityFileName))
                {
                    fileName = qualityFileName;
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(downloadName);
                    var ext = Path.GetExtension(downloadName);
                    downloadName = $"{nameWithoutExt}_{quality}{ext}";
                }
            }

            var filePath = _processingService.GetVideoPath(fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Video file not found");

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var mimeType = GetMimeType(Path.GetExtension(filePath));

            return File(fileStream, mimeType, downloadName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download video {VideoId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private string GetMimeType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".wmv" => "video/x-ms-wmv",
            ".flv" => "video/x-flv",
            ".mkv" => "video/x-matroska",
            ".webm" => "video/webm",
            _ => "application/octet-stream"
        };
    }
}
