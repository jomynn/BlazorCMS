using BlazorCMS.Shared.DTOs;
using FFMpegCore;
using FFMpegCore.Enums;

namespace BlazorCMS.API.Services;

public class VideoProcessingService : IVideoProcessingService
{
    private readonly ILogger<VideoProcessingService> _logger;

    public VideoProcessingService(ILogger<VideoProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> GenerateThumbnailAsync(Stream inputStream, TimeSpan? timestamp, int? width, int? height)
    {
        try
        {
            var tempVideoPath = Path.GetTempFileName() + ".mp4";
            var tempThumbnailPath = Path.GetTempFileName() + ".jpg";

            // Save video to temp file
            using (var fileStream = File.Create(tempVideoPath))
            {
                await inputStream.CopyToAsync(fileStream);
            }

            var videoInfo = await FFProbe.AnalyseAsync(tempVideoPath);
            var captureTime = timestamp ?? TimeSpan.FromSeconds(videoInfo.Duration.TotalSeconds / 2);

            // Generate thumbnail
            await FFMpeg.SnapshotAsync(tempVideoPath, tempThumbnailPath, captureTime: captureTime);

            // Resize if dimensions provided
            if (width.HasValue || height.HasValue)
            {
                var resizedPath = Path.GetTempFileName() + ".jpg";
                var size = new System.Drawing.Size(width ?? -1, height ?? -1);

                await FFMpegArguments
                    .FromFileInput(tempThumbnailPath)
                    .OutputToFile(resizedPath, true, options => options
                        .Resize(size))
                    .ProcessAsynchronously();

                File.Delete(tempThumbnailPath);
                tempThumbnailPath = resizedPath;
            }

            var thumbnailBytes = await File.ReadAllBytesAsync(tempThumbnailPath);

            // Cleanup
            File.Delete(tempVideoPath);
            File.Delete(tempThumbnailPath);

            return thumbnailBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating video thumbnail");
            throw;
        }
    }

    public async Task<byte[]> TrimVideoAsync(Stream inputStream, TimeSpan startTime, TimeSpan endTime, string outputFormat)
    {
        try
        {
            var tempInputPath = Path.GetTempFileName() + ".mp4";
            var tempOutputPath = Path.GetTempFileName() + $".{outputFormat}";

            // Save video to temp file
            using (var fileStream = File.Create(tempInputPath))
            {
                await inputStream.CopyToAsync(fileStream);
            }

            var duration = endTime - startTime;

            await FFMpegArguments
                .FromFileInput(tempInputPath, false, options => options
                    .Seek(startTime))
                .OutputToFile(tempOutputPath, true, options => options
                    .WithDuration(duration)
                    .WithVideoCodec("libx264")
                    .WithAudioCodec("aac"))
                .ProcessAsynchronously();

            var outputBytes = await File.ReadAllBytesAsync(tempOutputPath);

            // Cleanup
            File.Delete(tempInputPath);
            File.Delete(tempOutputPath);

            return outputBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trimming video");
            throw;
        }
    }

    public async Task<byte[]> ConvertVideoAsync(Stream inputStream, string outputFormat, int? bitrate)
    {
        try
        {
            var tempInputPath = Path.GetTempFileName() + ".tmp";
            var tempOutputPath = Path.GetTempFileName() + $".{outputFormat}";

            // Save video to temp file
            using (var fileStream = File.Create(tempInputPath))
            {
                await inputStream.CopyToAsync(fileStream);
            }

            var arguments = FFMpegArguments
                .FromFileInput(tempInputPath)
                .OutputToFile(tempOutputPath, true, options =>
                {
                    options.WithVideoCodec("libx264");
                    if (bitrate.HasValue)
                    {
                        options.WithVideoBitrate(bitrate.Value);
                    }
                });

            await arguments.ProcessAsynchronously();

            var outputBytes = await File.ReadAllBytesAsync(tempOutputPath);

            // Cleanup
            File.Delete(tempInputPath);
            File.Delete(tempOutputPath);

            return outputBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting video");
            throw;
        }
    }

    public async Task<byte[]> ConcatenateVideosAsync(List<Stream> videoStreams, string outputFormat)
    {
        try
        {
            var tempFiles = new List<string>();
            var tempOutputPath = Path.GetTempFileName() + $".{outputFormat}";

            // Save all videos to temp files
            foreach (var stream in videoStreams)
            {
                var tempPath = Path.GetTempFileName() + ".mp4";
                using (var fileStream = File.Create(tempPath))
                {
                    await stream.CopyToAsync(fileStream);
                }
                tempFiles.Add(tempPath);
            }

            // Concatenate using FFMpeg
            await FFMpeg.Join(tempOutputPath, tempFiles.ToArray());

            var outputBytes = await File.ReadAllBytesAsync(tempOutputPath);

            // Cleanup
            foreach (var file in tempFiles)
            {
                File.Delete(file);
            }
            File.Delete(tempOutputPath);

            return outputBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error concatenating videos");
            throw;
        }
    }
}
