using BlazorCMS.Data;
using BlazorCMS.Data.Models;
using Microsoft.EntityFrameworkCore;
using Xabe.FFmpeg;
using System.Text;

namespace BlazorCMS.Infrastructure.Services;

public interface IVideoMergeService
{
    Task<string> GetVideoMetadataAsync(string videoPath);
    Task<string> MergeVideosAsync(List<string> videoPaths, string outputPath, IProgress<int>? progress = null);
    Task<VideoMergeJob> ProcessMergeJobAsync(int jobId, IProgress<int>? progress = null);
}

public class VideoMergeService : IVideoMergeService
{
    private readonly ApplicationDbContext _context;
    private readonly string _storagePath;
    private readonly string _ffmpegPath;

    public VideoMergeService(ApplicationDbContext context)
    {
        _context = context;
        _storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads", "videos");
        _ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg");

        Directory.CreateDirectory(_storagePath);
        Directory.CreateDirectory(_ffmpegPath);

        // Set FFmpeg path for Xabe.FFmpeg
        FFmpeg.SetExecutablesPath(_ffmpegPath);
    }

    public async Task<string> GetVideoMetadataAsync(string videoPath)
    {
        try
        {
            IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
            IVideoStream videoStream = mediaInfo.VideoStreams.FirstOrDefault();
            IAudioStream audioStream = mediaInfo.AudioStreams.FirstOrDefault();

            var metadata = new StringBuilder();
            metadata.AppendLine($"Duration: {mediaInfo.Duration}");

            if (videoStream != null)
            {
                metadata.AppendLine($"Resolution: {videoStream.Width}x{videoStream.Height}");
                metadata.AppendLine($"Codec: {videoStream.Codec}");
                metadata.AppendLine($"Frame Rate: {videoStream.Framerate}");
                metadata.AppendLine($"Bitrate: {videoStream.Bitrate}");
            }

            if (audioStream != null)
            {
                metadata.AppendLine($"Audio Codec: {audioStream.Codec}");
                metadata.AppendLine($"Audio Bitrate: {audioStream.Bitrate}");
            }

            return metadata.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get video metadata: {ex.Message}", ex);
        }
    }

    public async Task<string> MergeVideosAsync(List<string> videoPaths, string outputPath, IProgress<int>? progress = null)
    {
        if (videoPaths == null || videoPaths.Count == 0)
            throw new ArgumentException("No video paths provided");

        if (videoPaths.Count == 1)
        {
            // If only one video, just copy it
            File.Copy(videoPaths[0], outputPath, true);
            return outputPath;
        }

        try
        {
            // Create a concat list file for FFmpeg
            var concatListPath = Path.Combine(Path.GetTempPath(), $"concat_{Guid.NewGuid()}.txt");
            var concatLines = videoPaths.Select(path => $"file '{path.Replace("\\", "/")}'");
            await File.WriteAllLinesAsync(concatListPath, concatLines);

            // Use concat demuxer for fast concatenation without re-encoding
            // This is much faster and maintains quality, especially for long videos
            var conversion = FFmpeg.Conversions.New()
                .AddParameter($"-f concat")
                .AddParameter($"-safe 0")
                .AddParameter($"-i \"{concatListPath}\"")
                .AddParameter("-c copy") // Copy streams without re-encoding (fast!)
                .SetOutput(outputPath);

            // Add progress tracking
            if (progress != null)
            {
                conversion.OnProgress += (sender, args) =>
                {
                    var percent = (int)((args.Duration.TotalSeconds / args.TotalLength.TotalSeconds) * 100);
                    progress.Report(Math.Min(percent, 100));
                };
            }

            await conversion.Start();

            // Clean up temp file
            if (File.Exists(concatListPath))
                File.Delete(concatListPath);

            return outputPath;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to merge videos: {ex.Message}", ex);
        }
    }

    public async Task<VideoMergeJob> ProcessMergeJobAsync(int jobId, IProgress<int>? progress = null)
    {
        var job = await _context.VideoMergeJobs.FindAsync(jobId);
        if (job == null)
            throw new ArgumentException($"Video merge job {jobId} not found");

        try
        {
            // Update job status
            job.Status = VideoMergeStatus.InProgress;
            job.Progress = 0;
            await _context.SaveChangesAsync();

            // Get all videos for this job
            var videos = await _context.Videos
                .Where(v => job.VideoIds.Contains(v.Id))
                .OrderBy(v => job.VideoIds.IndexOf(v.Id))
                .ToListAsync();

            if (videos.Count == 0)
                throw new Exception("No videos found for merge job");

            // Calculate total duration
            job.TotalDuration = TimeSpan.FromSeconds(videos.Sum(v => v.Duration.TotalSeconds));

            // Prepare video paths
            var videoPaths = videos.Select(v => Path.Combine(_storagePath, v.FilePath)).ToList();

            // Validate all files exist
            foreach (var path in videoPaths)
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException($"Video file not found: {path}");
            }

            // Generate output filename
            var outputFileName = $"merged_{job.JobId}_{DateTime.UtcNow:yyyyMMddHHmmss}.mp4";
            var outputPath = Path.Combine(_storagePath, outputFileName);
            job.OutputPath = outputFileName;

            // Create progress reporter
            var progressReporter = new Progress<int>(percent =>
            {
                job.Progress = percent;
                _context.SaveChanges();
                progress?.Report(percent);
            });

            // Merge videos
            await MergeVideosAsync(videoPaths, outputPath, progressReporter);

            // Update job status
            job.Status = VideoMergeStatus.Completed;
            job.Progress = 100;
            job.CompletedAt = DateTime.UtcNow;

            // Update video statuses
            foreach (var video in videos)
            {
                video.Status = VideoStatus.Merged;
                video.MergedVideoPath = outputFileName;
            }

            await _context.SaveChangesAsync();

            return job;
        }
        catch (Exception ex)
        {
            job.Status = VideoMergeStatus.Failed;
            job.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync();
            throw;
        }
    }
}
