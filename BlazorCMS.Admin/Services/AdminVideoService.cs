using BlazorCMS.Shared.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BlazorCMS.Admin.Services;

public class AdminVideoService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AdminVideoService> _logger;

    public AdminVideoService(HttpClient httpClient, ILogger<AdminVideoService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<VideoDTO>> GetAllVideosAsync()
    {
        try
        {
            var videos = await _httpClient.GetFromJsonAsync<List<VideoDTO>>("video");
            return videos ?? new List<VideoDTO>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get videos");
            return new List<VideoDTO>();
        }
    }

    public async Task<VideoDTO?> GetVideoByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<VideoDTO>($"video/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get video {VideoId}", id);
            return null;
        }
    }

    public async Task<VideoUploadResponseDTO> UploadVideoAsync(
        Stream fileStream,
        string fileName,
        string title,
        string? description,
        string? tags,
        bool isPublished)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
            content.Add(streamContent, "file", fileName);
            content.Add(new StringContent(title), "title");

            if (!string.IsNullOrEmpty(description))
                content.Add(new StringContent(description), "description");

            if (!string.IsNullOrEmpty(tags))
                content.Add(new StringContent(tags), "tags");

            content.Add(new StringContent(isPublished.ToString().ToLower()), "isPublished");

            var response = await _httpClient.PostAsync("video/upload", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<VideoUploadResponseDTO>();
                return result ?? new VideoUploadResponseDTO { Success = false, Message = "Unknown error" };
            }

            return new VideoUploadResponseDTO
            {
                Success = false,
                Message = $"Upload failed: {response.StatusCode}"
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

    public async Task<bool> UpdateVideoAsync(UpdateVideoDTO updateDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"video/{updateDto.Id}", updateDto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update video {VideoId}", updateDto.Id);
            return false;
        }
    }

    public async Task<bool> DeleteVideoAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"video/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete video {VideoId}", id);
            return false;
        }
    }

    public string GetVideoStreamUrl(int videoId, string? quality = "original")
    {
        var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "https://localhost:7250/api";
        return $"{baseUrl}/video/{videoId}/stream?quality={quality}";
    }

    public string GetThumbnailUrl(int videoId)
    {
        var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "https://localhost:7250/api";
        return $"{baseUrl}/video/{videoId}/thumbnail";
    }
}
