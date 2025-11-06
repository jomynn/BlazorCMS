using BlazorCMS.API.Services;
using BlazorCMS.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BlazorCMS.API.Controllers;

[Route("api/v1/media")]
[ApiController]
public class MediaController : ControllerBase
{
    private readonly IMediaMetadataService _metadataService;
    private readonly ILogger<MediaController> _logger;

    public MediaController(IMediaMetadataService metadataService, ILogger<MediaController> logger)
    {
        _metadataService = metadataService;
        _logger = logger;
    }

    /// <summary>
    /// Extract comprehensive metadata from media files
    /// </summary>
    [HttpPost("metadata")]
    public async Task<IActionResult> ExtractMetadata([FromForm] IFormFile file)
    {
        try
        {
            if (file == null)
            {
                return BadRequest("File is required");
            }

            using var inputStream = file.OpenReadStream();
            var result = await _metadataService.ExtractMetadataAsync(inputStream, file.FileName);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting metadata");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Convert media files from one format to another
    /// </summary>
    [HttpPost("convert")]
    public async Task<IActionResult> ConvertMedia([FromForm] MediaConvertRequestDTO request)
    {
        try
        {
            if (request.File == null && string.IsNullOrEmpty(request.SourceUrl))
            {
                return BadRequest("Either File or SourceUrl must be provided");
            }

            // This endpoint delegates to specific converters based on file type
            return Ok(new { message = "Use /api/v1/image/convert or /api/v1/video/convert for specific conversions" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting media");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
