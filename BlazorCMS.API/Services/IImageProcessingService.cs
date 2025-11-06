using BlazorCMS.Shared.DTOs;

namespace BlazorCMS.API.Services;

public interface IImageProcessingService
{
    Task<byte[]> ConvertImageAsync(Stream inputStream, ImageConvertRequestDTO request);
    Task<byte[]> ResizeImageAsync(Stream inputStream, int? width, int? height, bool maintainAspectRatio);
    Task<byte[]> CreateVideoFromImageAsync(Stream inputStream, int durationSeconds, string outputFormat);
}
