using BlazorCMS.API.Services;
using BlazorCMS.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BlazorCMS.API.Controllers;

[Route("api/v1/image")]
[ApiController]
public class ImageController : ControllerBase
{
    private readonly IImageProcessingService _imageService;
    private readonly ILogger<ImageController> _logger;

    public ImageController(IImageProcessingService imageService, ILogger<ImageController> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }

    /// <summary>
    /// Convert and resize an image
    /// </summary>
    [HttpPost("convert")]
    public async Task<IActionResult> ConvertImage([FromForm] ImageConvertRequestDTO request)
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

            var result = await _imageService.ConvertImageAsync(inputStream, request);

            return File(result, $"image/{request.OutputFormat}", $"converted.{request.OutputFormat}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting image");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Transforms a static image into a video with custom duration
    /// </summary>
    [HttpPost("convert/video")]
    public async Task<IActionResult> ConvertImageToVideo([FromForm] IFormFile file, [FromForm] int durationSeconds = 5, [FromForm] string outputFormat = "mp4")
    {
        try
        {
            if (file == null)
            {
                return BadRequest("File is required");
            }

            using var inputStream = file.OpenReadStream();
            var result = await _imageService.CreateVideoFromImageAsync(inputStream, durationSeconds, outputFormat);

            return File(result, $"video/{outputFormat}", $"video.{outputFormat}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting image to video");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Resize an image while maintaining aspect ratio
    /// </summary>
    [HttpPost("resize")]
    public async Task<IActionResult> ResizeImage([FromForm] IFormFile file, [FromForm] int? width, [FromForm] int? height, [FromForm] bool maintainAspectRatio = true)
    {
        try
        {
            if (file == null)
            {
                return BadRequest("File is required");
            }

            using var inputStream = file.OpenReadStream();
            var result = await _imageService.ResizeImageAsync(inputStream, width, height, maintainAspectRatio);

            return File(result, "image/jpeg", "resized.jpg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resizing image");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
