using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using Microsoft.Extensions.Logging;

namespace BlazorCMS.Infrastructure.Video;

public class VideoProcessingService
{
    private readonly ILogger<VideoProcessingService> _logger;
    private readonly string _videoStoragePath;
    private readonly string _thumbnailStoragePath;
    private static bool _ffmpegInitialized = false;
    private static readonly SemaphoreSlim _initLock = new(1, 1);

    public VideoProcessingService(ILogger<VideoProcessingService> logger)
    {
        _logger = logger;

        // Setup storage paths
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _videoStoragePath = Path.Combine(baseDirectory, "uploads", "videos");
        _thumbnailStoragePath = Path.Combine(baseDirectory, "uploads", "thumbnails");

        // Ensure directories exist
        Directory.CreateDirectory(_videoStoragePath);
        Directory.CreateDirectory(_thumbnailStoragePath);

        // Initialize FFmpeg if not already done
        _ = EnsureFfmpegAsync();
    }

    private async Task EnsureFfmpegAsync()
    {
        if (_ffmpegInitialized) return;

        await _initLock.WaitAsync();
        try
        {
            if (_ffmpegInitialized) return;

            var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg");
            Directory.CreateDirectory(ffmpegPath);

            // Download FFmpeg binaries if not present
            if (!File.Exists(Path.Combine(ffmpegPath, "ffmpeg.exe")) &&
                !File.Exists(Path.Combine(ffmpegPath, "ffmpeg")))
            {
                _logger.LogInformation("Downloading FFmpeg binaries...");
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegPath);
                _logger.LogInformation("FFmpeg binaries downloaded successfully.");
            }

            FFmpeg.SetExecutablesPath(ffmpegPath);
            _ffmpegInitialized = true;
            _logger.LogInformation("FFmpeg initialized at: {Path}", ffmpegPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize FFmpeg");
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Get video metadata information
    /// </summary>
    public async Task<VideoMetadata> GetVideoMetadataAsync(string filePath)
    {
        await EnsureFfmpegAsync();

        try
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(filePath);
            var videoStream = mediaInfo.VideoStreams.FirstOrDefault();
            var audioStream = mediaInfo.AudioStreams.FirstOrDefault();

            return new VideoMetadata
            {
                Duration = mediaInfo.Duration,
                VideoCodec = videoStream?.Codec,
                AudioCodec = audioStream?.Codec,
                Width = videoStream?.Width ?? 0,
                Height = videoStream?.Height ?? 0,
                FrameRate = videoStream?.Framerate ?? 0,
                BitRate = mediaInfo.Size,
                FileSize = new FileInfo(filePath).Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get video metadata for {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Generate thumbnail from video at specified time
    /// </summary>
    public async Task<string> GenerateThumbnailAsync(string videoPath, TimeSpan? atTime = null)
    {
        await EnsureFfmpegAsync();

        try
        {
            var fileName = Path.GetFileNameWithoutExtension(videoPath);
            var thumbnailFileName = $"{fileName}_thumb_{Guid.NewGuid()}.jpg";
            var thumbnailPath = Path.Combine(_thumbnailStoragePath, thumbnailFileName);

            var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
            var videoStream = mediaInfo.VideoStreams.FirstOrDefault();

            if (videoStream == null)
                throw new InvalidOperationException("No video stream found");

            // Default to 2 seconds or 10% of duration
            var seekTime = atTime ?? TimeSpan.FromSeconds(Math.Min(2, mediaInfo.Duration.TotalSeconds * 0.1));

            var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(videoPath, thumbnailPath, seekTime);
            await conversion.Start();

            _logger.LogInformation("Thumbnail generated: {ThumbnailPath}", thumbnailPath);
            return thumbnailFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate thumbnail for {VideoPath}", videoPath);
            throw;
        }
    }

    /// <summary>
    /// Convert video to different quality/resolution
    /// </summary>
    public async Task<string> ConvertVideoQualityAsync(
        string inputPath,
        string quality,
        IProgress<double>? progress = null)
    {
        await EnsureFfmpegAsync();

        try
        {
            var fileName = Path.GetFileNameWithoutExtension(inputPath);
            var outputFileName = $"{fileName}_{quality}_{Guid.NewGuid()}.mp4";
            var outputPath = Path.Combine(_videoStoragePath, outputFileName);

            var mediaInfo = await FFmpeg.GetMediaInfo(inputPath);
            var videoStream = mediaInfo.VideoStreams.FirstOrDefault();
            var audioStream = mediaInfo.AudioStreams.FirstOrDefault();

            if (videoStream == null)
                throw new InvalidOperationException("No video stream found");

            // Set resolution based on quality
            var (width, height, bitrate) = quality.ToLower() switch
            {
                "1080p" => (1920, 1080, "5000k"),
                "720p" => (1280, 720, "2500k"),
                "480p" => (854, 480, "1000k"),
                "360p" => (640, 360, "500k"),
                _ => throw new ArgumentException($"Unsupported quality: {quality}")
            };

            // Only scale if original is larger
            if (videoStream.Width <= width && videoStream.Height <= height)
            {
                _logger.LogInformation("Source video is smaller than target {Quality}, copying instead", quality);
                File.Copy(inputPath, outputPath);
                return outputFileName;
            }

            var conversion = FFmpeg.Conversions.New()
                .AddStream(videoStream)
                .AddStream(audioStream)
                .SetOutput(outputPath)
                .SetVideoCodec(VideoCodec.h264)
                .SetVideoBitrate(bitrate)
                .SetSize(new VideoSize(width, height))
                .SetPreset(ConversionPreset.Medium);

            conversion.OnProgress += (sender, args) =>
            {
                var percent = (args.Duration.TotalSeconds / args.TotalLength.TotalSeconds) * 100;
                progress?.Report(percent);
                _logger.LogDebug("Conversion progress: {Percent:F2}%", percent);
            };

            await conversion.Start();

            _logger.LogInformation("Video converted to {Quality}: {OutputPath}", quality, outputPath);
            return outputFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert video to {Quality}", quality);
            throw;
        }
    }

    /// <summary>
    /// Extract audio from video
    /// </summary>
    public async Task<string> ExtractAudioAsync(string videoPath, string format = "mp3")
    {
        await EnsureFfmpegAsync();

        try
        {
            var fileName = Path.GetFileNameWithoutExtension(videoPath);
            var audioFileName = $"{fileName}_audio_{Guid.NewGuid()}.{format}";
            var audioPath = Path.Combine(_videoStoragePath, audioFileName);

            var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
            var audioStream = mediaInfo.AudioStreams.FirstOrDefault();

            if (audioStream == null)
                throw new InvalidOperationException("No audio stream found");

            var conversion = FFmpeg.Conversions.New()
                .AddStream(audioStream)
                .SetOutput(audioPath);

            await conversion.Start();

            _logger.LogInformation("Audio extracted: {AudioPath}", audioPath);
            return audioFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract audio from {VideoPath}", videoPath);
            throw;
        }
    }

    /// <summary>
    /// Add watermark text to video
    /// </summary>
    public async Task<string> AddWatermarkAsync(string videoPath, string watermarkText)
    {
        await EnsureFfmpegAsync();

        try
        {
            var fileName = Path.GetFileNameWithoutExtension(videoPath);
            var outputFileName = $"{fileName}_watermarked_{Guid.NewGuid()}.mp4";
            var outputPath = Path.Combine(_videoStoragePath, outputFileName);

            var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
            var videoStream = mediaInfo.VideoStreams.FirstOrDefault();
            var audioStream = mediaInfo.AudioStreams.FirstOrDefault();

            if (videoStream == null)
                throw new InvalidOperationException("No video stream found");

            // Add text watermark using drawtext filter
            var watermarkFilter = $"drawtext=text='{watermarkText}':fontsize=24:fontcolor=white@0.5:x=10:y=10";

            var conversion = FFmpeg.Conversions.New()
                .AddStream(videoStream)
                .AddStream(audioStream)
                .AddParameter($"-vf \"{watermarkFilter}\"")
                .SetOutput(outputPath);

            await conversion.Start();

            _logger.LogInformation("Watermark added: {OutputPath}", outputPath);
            return outputFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add watermark to {VideoPath}", videoPath);
            throw;
        }
    }

    /// <summary>
    /// Trim video to specified time range
    /// </summary>
    public async Task<string> TrimVideoAsync(string videoPath, TimeSpan startTime, TimeSpan duration)
    {
        await EnsureFfmpegAsync();

        try
        {
            var fileName = Path.GetFileNameWithoutExtension(videoPath);
            var outputFileName = $"{fileName}_trimmed_{Guid.NewGuid()}.mp4";
            var outputPath = Path.Combine(_videoStoragePath, outputFileName);

            var conversion = await FFmpeg.Conversions.FromSnippet.Split(videoPath, outputPath, startTime, duration);
            await conversion.Start();

            _logger.LogInformation("Video trimmed: {OutputPath}", outputPath);
            return outputFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trim video {VideoPath}", videoPath);
            throw;
        }
    }

    /// <summary>
    /// Concatenate multiple videos
    /// </summary>
    public async Task<string> ConcatenateVideosAsync(List<string> videoPaths)
    {
        await EnsureFfmpegAsync();

        try
        {
            var outputFileName = $"concatenated_{Guid.NewGuid()}.mp4";
            var outputPath = Path.Combine(_videoStoragePath, outputFileName);

            var mediaInfos = new List<IMediaInfo>();
            foreach (var path in videoPaths)
            {
                var fullPath = Path.Combine(_videoStoragePath, path);
                mediaInfos.Add(await FFmpeg.GetMediaInfo(fullPath));
            }

            var conversion = FFmpeg.Conversions.New();

            foreach (var mediaInfo in mediaInfos)
            {
                conversion.AddStream(mediaInfo.VideoStreams.First());
                conversion.AddStream(mediaInfo.AudioStreams.FirstOrDefault());
            }

            conversion.SetOutput(outputPath)
                .SetOutputFormat(Format.mp4);

            await conversion.Start();

            _logger.LogInformation("Videos concatenated: {OutputPath}", outputPath);
            return outputFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to concatenate videos");
            throw;
        }
    }

    /// <summary>
    /// Get full path to video file
    /// </summary>
    public string GetVideoPath(string fileName)
    {
        return Path.Combine(_videoStoragePath, fileName);
    }

    /// <summary>
    /// Get full path to thumbnail file
    /// </summary>
    public string GetThumbnailPath(string fileName)
    {
        return Path.Combine(_thumbnailStoragePath, fileName);
    }
}

public class VideoMetadata
{
    public TimeSpan Duration { get; set; }
    public string? VideoCodec { get; set; }
    public string? AudioCodec { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double FrameRate { get; set; }
    public long BitRate { get; set; }
    public long FileSize { get; set; }
}
