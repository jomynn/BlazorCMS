using BlazorCMS.Shared.DTOs;

namespace BlazorCMS.API.Services;

public interface IVideoProcessingService
{
    Task<byte[]> GenerateThumbnailAsync(Stream inputStream, TimeSpan? timestamp, int? width, int? height);
    Task<byte[]> TrimVideoAsync(Stream inputStream, TimeSpan startTime, TimeSpan endTime, string outputFormat);
    Task<byte[]> ConvertVideoAsync(Stream inputStream, string outputFormat, int? bitrate);
    Task<byte[]> ConcatenateVideosAsync(List<Stream> videoStreams, string outputFormat);
}
