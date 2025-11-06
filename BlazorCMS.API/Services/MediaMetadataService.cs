using BlazorCMS.Shared.DTOs;
using FFMpegCore;
using MetadataExtractor;
using SixLabors.ImageSharp;

namespace BlazorCMS.API.Services;

public class MediaMetadataService : IMediaMetadataService
{
    private readonly ILogger<MediaMetadataService> _logger;

    public MediaMetadataService(ILogger<MediaMetadataService> logger)
    {
        _logger = logger;
    }

    public async Task<MediaMetadataResponseDTO> ExtractMetadataAsync(Stream inputStream, string fileName)
    {
        try
        {
            var extension = Path.GetExtension(fileName).ToLower();
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv", ".wmv", ".flv" };

            if (imageExtensions.Contains(extension))
            {
                return await ExtractImageMetadataAsync(inputStream, fileName);
            }
            else if (videoExtensions.Contains(extension))
            {
                return await ExtractVideoMetadataAsync(inputStream, fileName);
            }
            else
            {
                // Generic file metadata
                return new MediaMetadataResponseDTO
                {
                    FileName = fileName,
                    FileSize = inputStream.Length,
                    MimeType = GetMimeType(extension)
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting metadata");
            throw;
        }
    }

    public async Task<MediaMetadataResponseDTO> ExtractVideoMetadataAsync(Stream inputStream, string fileName)
    {
        try
        {
            var tempPath = Path.GetTempFileName() + Path.GetExtension(fileName);

            // Save to temp file for FFProbe
            using (var fileStream = File.Create(tempPath))
            {
                await inputStream.CopyToAsync(fileStream);
            }

            var videoInfo = await FFProbe.AnalyseAsync(tempPath);

            var metadata = new MediaMetadataResponseDTO
            {
                FileName = fileName,
                FileSize = new FileInfo(tempPath).Length,
                MimeType = GetMimeType(Path.GetExtension(fileName)),
                Duration = videoInfo.Duration,
                Format = videoInfo.Format.FormatName,
                Width = videoInfo.PrimaryVideoStream?.Width,
                Height = videoInfo.PrimaryVideoStream?.Height,
                FrameRate = videoInfo.PrimaryVideoStream?.FrameRate ?? 0,
                Bitrate = (int?)videoInfo.Format.BitRate,
                VideoCodec = videoInfo.PrimaryVideoStream?.CodecName,
                AudioCodec = videoInfo.PrimaryAudioStream?.CodecName,
                AdditionalMetadata = new Dictionary<string, string>
                {
                    ["VideoStreams"] = videoInfo.VideoStreams.Count.ToString(),
                    ["AudioStreams"] = videoInfo.AudioStreams.Count.ToString(),
                    ["FormatLongName"] = videoInfo.Format.FormatLongName ?? ""
                }
            };

            // Cleanup
            File.Delete(tempPath);

            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting video metadata");
            throw;
        }
    }

    public async Task<MediaMetadataResponseDTO> ExtractImageMetadataAsync(Stream inputStream, string fileName)
    {
        try
        {
            // Reset stream position
            if (inputStream.CanSeek)
            {
                inputStream.Position = 0;
            }

            using var image = await Image.LoadAsync(inputStream);

            var metadata = new MediaMetadataResponseDTO
            {
                FileName = fileName,
                FileSize = inputStream.Length,
                MimeType = GetMimeType(Path.GetExtension(fileName)),
                Width = image.Width,
                Height = image.Height,
                Format = image.Metadata.DecodedImageFormat?.Name ?? "Unknown",
                AdditionalMetadata = new Dictionary<string, string>
                {
                    ["HorizontalResolution"] = image.Metadata.HorizontalResolution.ToString(),
                    ["VerticalResolution"] = image.Metadata.VerticalResolution.ToString()
                }
            };

            // Extract EXIF data if available
            if (inputStream.CanSeek)
            {
                inputStream.Position = 0;
                try
                {
                    var directories = ImageMetadataReader.ReadMetadata(inputStream);
                    foreach (var directory in directories)
                    {
                        foreach (var tag in directory.Tags)
                        {
                            metadata.AdditionalMetadata[$"{directory.Name}_{tag.Name}"] = tag.Description ?? "";
                        }
                    }
                }
                catch
                {
                    // EXIF extraction failed, continue without it
                }
            }

            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting image metadata");
            throw;
        }
    }

    private static string GetMimeType(string extension)
    {
        return extension.ToLower() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".mkv" => "video/x-matroska",
            ".wmv" => "video/x-ms-wmv",
            ".flv" => "video/x-flv",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            _ => "application/octet-stream"
        };
    }
}
