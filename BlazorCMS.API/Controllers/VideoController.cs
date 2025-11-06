using BlazorCMS.Data;
using BlazorCMS.Data.Models;
using BlazorCMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlazorCMS.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class VideoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IChunkedUploadService _uploadService;
    private readonly IVideoMergeService _mergeService;
    private readonly ILogger<VideoController> _logger;

    public VideoController(
        ApplicationDbContext context,
        IChunkedUploadService uploadService,
        IVideoMergeService mergeService,
        ILogger<VideoController> logger)
    {
        _context = context;
        _uploadService = uploadService;
        _mergeService = mergeService;
        _logger = logger;
    }

    // POST: api/video/upload-chunk
    [HttpPost("upload-chunk")]
    [RequestSizeLimit(104857600)] // 100MB per chunk
    public async Task<IActionResult> UploadChunk(
        [FromForm] string uploadId,
        [FromForm] int chunkIndex,
        [FromForm] int totalChunks,
        IFormFile chunk)
    {
        try
        {
            if (chunk == null || chunk.Length == 0)
                return BadRequest("No chunk data provided");

            await _uploadService.SaveChunkAsync(uploadId, chunkIndex, totalChunks, chunk);

            return Ok(new
            {
                success = true,
                uploadId,
                chunkIndex,
                message = $"Chunk {chunkIndex + 1}/{totalChunks} uploaded successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload chunk {ChunkIndex} for upload {UploadId}", chunkIndex, uploadId);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // POST: api/video/finalize-upload
    [HttpPost("finalize-upload")]
    public async Task<IActionResult> FinalizeUpload([FromBody] FinalizeUploadRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            var video = await _uploadService.FinalizeUploadAsync(request.UploadId, request.TotalChunks, userId);

            return Ok(new
            {
                success = true,
                video = new
                {
                    video.Id,
                    video.FileName,
                    video.Duration,
                    video.Resolution,
                    video.FileSizeBytes,
                    video.Status
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to finalize upload {UploadId}", request.UploadId);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // DELETE: api/video/cancel-upload/{uploadId}
    [HttpDelete("cancel-upload/{uploadId}")]
    public async Task<IActionResult> CancelUpload(string uploadId)
    {
        try
        {
            await _uploadService.DeleteUploadAsync(uploadId);
            return Ok(new { success = true, message = "Upload cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel upload {UploadId}", uploadId);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // GET: api/video
    [HttpGet]
    public async Task<IActionResult> GetAllVideos([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var query = _context.Videos.AsQueryable();

            // Optionally filter by user
            // query = query.Where(v => v.UploadedBy == userId);

            var totalCount = await query.CountAsync();
            var videos = await query
                .OrderByDescending(v => v.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new
                {
                    v.Id,
                    v.FileName,
                    v.Duration,
                    v.Resolution,
                    v.FileSizeBytes,
                    v.Status,
                    v.UploadedAt,
                    v.MergedVideoPath
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                videos,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get videos");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // GET: api/video/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVideo(int id)
    {
        try
        {
            var video = await _context.Videos.FindAsync(id);
            if (video == null)
                return NotFound(new { success = false, message = "Video not found" });

            return Ok(new { success = true, video });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get video {VideoId}", id);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // POST: api/video/merge
    [HttpPost("merge")]
    public async Task<IActionResult> CreateMergeJob([FromBody] CreateMergeJobRequest request)
    {
        try
        {
            if (request.VideoIds == null || request.VideoIds.Count < 2)
                return BadRequest(new { success = false, message = "At least 2 videos are required for merging" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

            // Verify all videos exist
            var videos = await _context.Videos
                .Where(v => request.VideoIds.Contains(v.Id))
                .ToListAsync();

            if (videos.Count != request.VideoIds.Count)
                return BadRequest(new { success = false, message = "Some videos not found" });

            // Create merge job
            var job = new VideoMergeJob
            {
                JobId = Guid.NewGuid().ToString(),
                VideoIds = request.VideoIds,
                Status = VideoMergeStatus.Pending,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.VideoMergeJobs.Add(job);
            await _context.SaveChangesAsync();

            // Start merge process in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _mergeService.ProcessMergeJobAsync(job.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process merge job {JobId}", job.Id);
                }
            });

            return Ok(new
            {
                success = true,
                job = new
                {
                    job.Id,
                    job.JobId,
                    job.Status,
                    job.VideoIds,
                    job.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create merge job");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // GET: api/video/merge-job/{id}
    [HttpGet("merge-job/{id}")]
    public async Task<IActionResult> GetMergeJob(int id)
    {
        try
        {
            var job = await _context.VideoMergeJobs.FindAsync(id);
            if (job == null)
                return NotFound(new { success = false, message = "Merge job not found" });

            return Ok(new { success = true, job });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get merge job {JobId}", id);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // GET: api/video/merge-jobs
    [HttpGet("merge-jobs")]
    public async Task<IActionResult> GetAllMergeJobs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var totalCount = await _context.VideoMergeJobs.CountAsync();
            var jobs = await _context.VideoMergeJobs
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                jobs,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get merge jobs");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // DELETE: api/video/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVideo(int id)
    {
        try
        {
            var video = await _context.Videos.FindAsync(id);
            if (video == null)
                return NotFound(new { success = false, message = "Video not found" });

            // Delete physical file
            var filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "uploads",
                "videos",
                video.FilePath);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.Videos.Remove(video);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Video deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete video {VideoId}", id);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

public class FinalizeUploadRequest
{
    public string UploadId { get; set; } = string.Empty;
    public int TotalChunks { get; set; }
}

public class CreateMergeJobRequest
{
    public List<int> VideoIds { get; set; } = new();
}
