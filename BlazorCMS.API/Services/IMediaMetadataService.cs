using BlazorCMS.Shared.DTOs;

namespace BlazorCMS.API.Services;

public interface IMediaMetadataService
{
    Task<MediaMetadataResponseDTO> ExtractMetadataAsync(Stream inputStream, string fileName);
    Task<MediaMetadataResponseDTO> ExtractVideoMetadataAsync(Stream inputStream, string fileName);
    Task<MediaMetadataResponseDTO> ExtractImageMetadataAsync(Stream inputStream, string fileName);
}
