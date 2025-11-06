using BlazorCMS.API.Services;
using BlazorCMS.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BlazorCMS.API.Controllers;

[Route("api/v1/video")]
[ApiController]
public class VideoController : ControllerBase
{
    private readonly IVideoProcessingService _videoService;
    private readonly ILogger<VideoController> _logger;

    public VideoController(IVideoProcessingService videoService, ILogger<VideoController> logger)
    {
        _videoService = videoService;
        _logger = logger;
    }

    /// <summary>
    /// Extract a thumbnail image from a video at a specific timestamp
    /// </summary>
    [HttpPost("thumbnail")]
    public async Task<IActionResult> GenerateThumbnail([FromForm] VideoThumbnailRequestDTO request)
    {
        try
        {
            if (request.File == null && string.IsNullOrEmpty(request.SourceUrl))
            {
                return BadRequest("Either File or SourceUrl must be provided");
            }

            Stream inputStream;

            if (request.File != null)
            {
                inputStream = request.File.OpenReadStream();
            }
            else
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(request.SourceUrl);
                response.EnsureSuccessStatusCode();
                inputStream = await response.Content.ReadAsStreamAsync();
            }

            var result = await _videoService.GenerateThumbnailAsync(inputStream, request.Timestamp, request.Width, request.Height);

            return File(result, "image/jpeg", "thumbnail.jpg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnail");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Trim a video by keeping only the content between specified start and end times
    /// </summary>
    [HttpPost("trim")]
    public async Task<IActionResult> TrimVideo([FromForm] VideoTrimRequestDTO request)
    {
        try
        {
            if (request.File == null && string.IsNullOrEmpty(request.SourceUrl))
            {
                return BadRequest("Either File or SourceUrl must be provided");
            }

            Stream inputStream;

            if (request.File != null)
            {
                inputStream = request.File.OpenReadStream();
            }
            else
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(request.SourceUrl);
                response.EnsureSuccessStatusCode();
                inputStream = await response.Content.ReadAsStreamAsync();
            }

            var result = await _videoService.TrimVideoAsync(inputStream, request.StartTime, request.EndTime, request.OutputFormat);

            return File(result, $"video/{request.OutputFormat}", $"trimmed.{request.OutputFormat}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trimming video");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Convert video to different format
    /// </summary>
    [HttpPost("convert")]
    public async Task<IActionResult> ConvertVideo([FromForm] MediaConvertRequestDTO request)
    {
        try
        {
            if (request.File == null && string.IsNullOrEmpty(request.SourceUrl))
            {
                return BadRequest("Either File or SourceUrl must be provided");
            }

            Stream inputStream;

            if (request.File != null)
            {
                inputStream = request.File.OpenReadStream();
            }
            else
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(request.SourceUrl);
                response.EnsureSuccessStatusCode();
                inputStream = await response.Content.ReadAsStreamAsync();
            }

            var result = await _videoService.ConvertVideoAsync(inputStream, request.OutputFormat, request.Bitrate);

            return File(result, $"video/{request.OutputFormat}", $"converted.{request.OutputFormat}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting video");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
