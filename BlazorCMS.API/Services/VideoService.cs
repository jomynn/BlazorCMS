using BlazorCMS.Data.Models;
using BlazorCMS.Data.Repositories;
using BlazorCMS.Infrastructure.Video;
using BlazorCMS.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace BlazorCMS.API.Services;

public class VideoService
{
    private readonly IVideoRepository _videoRepository;
    private readonly VideoProcessingService _processingService;
    private readonly ILogger<VideoService> _logger;

    public VideoService(
        IVideoRepository videoRepository,
        VideoProcessingService processingService,
        ILogger<VideoService> logger)
    {
        _videoRepository = videoRepository;
        _processingService = processingService;
        _logger = logger;
    }

    public async Task<VideoDTO?> GetByIdAsync(int id)
    {
        var video = await _videoRepository.GetByIdAsync(id);
        return video != null ? MapToDTO(video) : null;
    }

    public async Task<List<VideoDTO>> GetAllAsync()
    {
        var videos = await _videoRepository.GetAllAsync();
        return videos.Select(MapToDTO).ToList();
    }

    public async Task<List<VideoDTO>> GetPublishedAsync()
    {
        var videos = await _videoRepository.GetPublishedAsync();
        return videos.Select(MapToDTO).ToList();
    }

    public async Task<List<VideoDTO>> GetByAuthorAsync(string authorId)
    {
        var videos = await _videoRepository.GetByAuthorAsync(authorId);
        return videos.Select(MapToDTO).ToList();
    }

    public async Task<VideoUploadResponseDTO> UploadVideoAsync(
        IFormFile file,
        CreateVideoDTO createDto,
        string? authorId,
        string? authorName)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return new VideoUploadResponseDTO
                {
                    Success = false,
                    Message = "No file provided"
                };
            }

            // Validate file type
            var allowedExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return new VideoUploadResponseDTO
                {
                    Success = false,
                    Message = $"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}"
                };
            }

            // Save uploaded file
            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var videoPath = _processingService.GetVideoPath(storedFileName);

            using (var stream = new FileStream(videoPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Video file saved: {FileName}", storedFileName);

            // Create video record
            var video = new Video
            {
                Title = createDto.Title,
                Description = createDto.Description,
                OriginalFileName = file.FileName,
                StoredFileName = storedFileName,
                Tags = createDto.Tags,
                IsPublished = createDto.IsPublished,
                AuthorId = authorId,
                Author = authorName,
                ProcessingStatus = "Pending",
                FileSizeBytes = file.Length,
                CreatedAt = DateTime.UtcNow
            };

            video = await _videoRepository.CreateAsync(video);

            // Process video in background (fire and forget with proper error handling)
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessVideoAsync(video.Id, videoPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background video processing failed for video {VideoId}", video.Id);
                }
            });

            return new VideoUploadResponseDTO
            {
                Success = true,
                Message = "Video uploaded successfully. Processing started.",
                Video = MapToDTO(video)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload video");
            return new VideoUploadResponseDTO
            {
                Success = false,
                Message = $"Upload failed: {ex.Message}"
            };
        }
    }

    private async Task ProcessVideoAsync(int videoId, string videoPath)
    {
        try
        {
            var video = await _videoRepository.GetByIdAsync(videoId);
            if (video == null)
            {
                _logger.LogWarning("Video {VideoId} not found for processing", videoId);
                return;
            }

            video.ProcessingStatus = "Processing";
            await _videoRepository.UpdateAsync(video);

            _logger.LogInformation("Starting video processing for {VideoId}", videoId);

            // 1. Extract metadata
            var metadata = await _processingService.GetVideoMetadataAsync(videoPath);
            video.DurationSeconds = metadata.Duration.TotalSeconds;
            video.VideoCodec = metadata.VideoCodec;
            video.AudioCodec = metadata.AudioCodec;
            video.Width = metadata.Width;
            video.Height = metadata.Height;
            video.FrameRate = metadata.FrameRate.ToString("F2");
            video.BitRate = metadata.BitRate;

            _logger.LogInformation("Metadata extracted for {VideoId}: {Width}x{Height}, {Duration}s",
                videoId, metadata.Width, metadata.Height, metadata.Duration.TotalSeconds);

            // 2. Generate thumbnail
            var thumbnailFileName = await _processingService.GenerateThumbnailAsync(videoPath);
            video.ThumbnailFileName = thumbnailFileName;

            _logger.LogInformation("Thumbnail generated for {VideoId}: {ThumbnailFileName}",
                videoId, thumbnailFileName);

            // 3. Generate multiple quality versions
            var processedVersions = new Dictionary<string, string>();
            var qualities = new[] { "1080p", "720p", "480p" };

            foreach (var quality in qualities)
            {
                // Only process if source is high enough quality
                if ((quality == "1080p" && metadata.Height >= 1080) ||
                    (quality == "720p" && metadata.Height >= 720) ||
                    (quality == "480p" && metadata.Height >= 480))
                {
                    try
                    {
                        _logger.LogInformation("Converting {VideoId} to {Quality}", videoId, quality);
                        var convertedFileName = await _processingService.ConvertVideoQualityAsync(
                            videoPath, quality);
                        processedVersions[quality] = convertedFileName;
                        _logger.LogInformation("{Quality} version created for {VideoId}: {FileName}",
                            quality, videoId, convertedFileName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to convert {VideoId} to {Quality}", videoId, quality);
                    }
                }
            }

            // Always include original
            processedVersions["original"] = video.StoredFileName;
            video.ProcessedVersions = JsonSerializer.Serialize(processedVersions);

            // 4. Mark as completed
            video.ProcessingStatus = "Completed";
            if (video.IsPublished && !video.PublishedDate.HasValue)
            {
                video.PublishedDate = DateTime.UtcNow;
            }

            await _videoRepository.UpdateAsync(video);

            _logger.LogInformation("Video processing completed for {VideoId}", videoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Video processing failed for {VideoId}", videoId);

            var video = await _videoRepository.GetByIdAsync(videoId);
            if (video != null)
            {
                video.ProcessingStatus = "Failed";
                video.ProcessingError = ex.Message;
                await _videoRepository.UpdateAsync(video);
            }
        }
    }

    public async Task<VideoDTO?> UpdateAsync(UpdateVideoDTO updateDto)
    {
        var video = await _videoRepository.GetByIdAsync(updateDto.Id);
        if (video == null)
            return null;

        video.Title = updateDto.Title;
        video.Description = updateDto.Description;
        video.Tags = updateDto.Tags;
        video.IsPublished = updateDto.IsPublished;

        if (updateDto.IsPublished && !video.PublishedDate.HasValue)
        {
            video.PublishedDate = DateTime.UtcNow;
        }

        video = await _videoRepository.UpdateAsync(video);
        return MapToDTO(video);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var video = await _videoRepository.GetByIdAsync(id);
            if (video == null)
                return false;

            // Delete physical files
            try
            {
                var videoPath = _processingService.GetVideoPath(video.StoredFileName);
                if (File.Exists(videoPath))
                    File.Delete(videoPath);

                if (!string.IsNullOrEmpty(video.ThumbnailFileName))
                {
                    var thumbnailPath = _processingService.GetThumbnailPath(video.ThumbnailFileName);
                    if (File.Exists(thumbnailPath))
                        File.Delete(thumbnailPath);
                }

                // Delete processed versions
                if (!string.IsNullOrEmpty(video.ProcessedVersions))
                {
                    var versions = JsonSerializer.Deserialize<Dictionary<string, string>>(video.ProcessedVersions);
                    if (versions != null)
                    {
                        foreach (var versionFile in versions.Values)
                        {
                            var versionPath = _processingService.GetVideoPath(versionFile);
                            if (File.Exists(versionPath))
                                File.Delete(versionPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete physical files for video {VideoId}", id);
            }

            return await _videoRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete video {VideoId}", id);
            return false;
        }
    }

    public async Task<List<VideoDTO>> SearchAsync(string searchTerm)
    {
        var videos = await _videoRepository.SearchAsync(searchTerm);
        return videos.Select(MapToDTO).ToList();
    }

    public async Task IncrementViewCountAsync(int id)
    {
        await _videoRepository.IncrementViewCountAsync(id);
    }

    private VideoDTO MapToDTO(Video video)
    {
        Dictionary<string, string>? processedVersions = null;
        if (!string.IsNullOrEmpty(video.ProcessedVersions))
        {
            try
            {
                processedVersions = JsonSerializer.Deserialize<Dictionary<string, string>>(video.ProcessedVersions);
            }
            catch
            {
                // Ignore deserialization errors
            }
        }

        return new VideoDTO
        {
            Id = video.Id,
            Title = video.Title,
            Description = video.Description,
            OriginalFileName = video.OriginalFileName,
            StoredFileName = video.StoredFileName,
            ThumbnailFileName = video.ThumbnailFileName,
            DurationSeconds = video.DurationSeconds,
            VideoCodec = video.VideoCodec,
            AudioCodec = video.AudioCodec,
            Width = video.Width,
            Height = video.Height,
            FileSizeBytes = video.FileSizeBytes,
            FrameRate = video.FrameRate,
            BitRate = video.BitRate,
            ProcessedVersions = processedVersions,
            IsPublished = video.IsPublished,
            CreatedAt = video.CreatedAt,
            PublishedDate = video.PublishedDate,
            ProcessingStatus = video.ProcessingStatus,
            ProcessingError = video.ProcessingError,
            AuthorId = video.AuthorId,
            Author = video.Author,
            Tags = video.Tags,
            ViewCount = video.ViewCount
        };
    }
}
