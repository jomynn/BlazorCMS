using BlazorCMS.Data;
using BlazorCMS.Data.Models;
using Microsoft.AspNetCore.Http;
using Xabe.FFmpeg;

namespace BlazorCMS.Infrastructure.Services;

public interface IChunkedUploadService
{
    Task<string> SaveChunkAsync(string uploadId, int chunkIndex, int totalChunks, IFormFile chunk);
    Task<Video> FinalizeUploadAsync(string uploadId, int totalChunks, string userId);
    Task<bool> DeleteUploadAsync(string uploadId);
}

public class ChunkedUploadService : IChunkedUploadService
{
    private readonly ApplicationDbContext _context;
    private readonly string _tempPath;
    private readonly string _storagePath;

    public ChunkedUploadService(ApplicationDbContext context)
    {
        _context = context;
        _tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads", "temp");
        _storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads", "videos");

        Directory.CreateDirectory(_tempPath);
        Directory.CreateDirectory(_storagePath);
    }

    public async Task<string> SaveChunkAsync(string uploadId, int chunkIndex, int totalChunks, IFormFile chunk)
    {
        try
        {
            var uploadDir = Path.Combine(_tempPath, uploadId);
            Directory.CreateDirectory(uploadDir);

            var chunkPath = Path.Combine(uploadDir, $"chunk_{chunkIndex:D6}");

            using (var stream = new FileStream(chunkPath, FileMode.Create))
            {
                await chunk.CopyToAsync(stream);
            }

            return chunkPath;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to save chunk {chunkIndex}: {ex.Message}", ex);
        }
    }

    public async Task<Video> FinalizeUploadAsync(string uploadId, int totalChunks, string userId)
    {
        try
        {
            var uploadDir = Path.Combine(_tempPath, uploadId);

            if (!Directory.Exists(uploadDir))
                throw new DirectoryNotFoundException($"Upload directory not found for {uploadId}");

            // Verify all chunks are present
            for (int i = 0; i < totalChunks; i++)
            {
                var chunkPath = Path.Combine(uploadDir, $"chunk_{i:D6}");
                if (!File.Exists(chunkPath))
                    throw new FileNotFoundException($"Missing chunk {i}");
            }

            // Combine chunks into final video file
            var finalFileName = $"{uploadId}.mp4";
            var finalPath = Path.Combine(_storagePath, finalFileName);

            using (var outputStream = new FileStream(finalPath, FileMode.Create))
            {
                for (int i = 0; i < totalChunks; i++)
                {
                    var chunkPath = Path.Combine(uploadDir, $"chunk_{i:D6}");
                    using (var chunkStream = new FileStream(chunkPath, FileMode.Open))
                    {
                        await chunkStream.CopyToAsync(outputStream);
                    }
                }
            }

            // Get video metadata using FFmpeg
            var mediaInfo = await FFmpeg.GetMediaInfo(finalPath);
            var videoStream = mediaInfo.VideoStreams.FirstOrDefault();
            var audioStream = mediaInfo.AudioStreams.FirstOrDefault();

            // Create Video entity
            var video = new Video
            {
                FileName = finalFileName,
                FilePath = finalFileName,
                FileSizeBytes = new FileInfo(finalPath).Length,
                Duration = mediaInfo.Duration,
                Resolution = videoStream != null ? $"{videoStream.Width}x{videoStream.Height}" : null,
                Codec = videoStream?.Codec,
                FrameRate = videoStream?.Framerate ?? 0,
                AudioCodec = audioStream?.Codec,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = userId,
                Status = VideoStatus.Uploaded
            };

            _context.Videos.Add(video);
            await _context.SaveChangesAsync();

            // Clean up temp directory
            Directory.Delete(uploadDir, true);

            return video;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to finalize upload: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteUploadAsync(string uploadId)
    {
        try
        {
            var uploadDir = Path.Combine(_tempPath, uploadId);
            if (Directory.Exists(uploadDir))
            {
                Directory.Delete(uploadDir, true);
            }
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete upload: {ex.Message}", ex);
        }
    }
}
