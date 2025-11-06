using BlazorCMS.Shared.DTOs;
using System.Net.Http.Json;

namespace BlazorCMS.Client.Services;

public class ClientVideoService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClientVideoService> _logger;

    public ClientVideoService(HttpClient httpClient, ILogger<ClientVideoService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<VideoDTO>> GetPublishedVideosAsync()
    {
        try
        {
            var videos = await _httpClient.GetFromJsonAsync<List<VideoDTO>>("video/published");
            return videos ?? new List<VideoDTO>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get published videos");
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

    public async Task<List<VideoDTO>> SearchVideosAsync(string searchTerm)
    {
        try
        {
            var videos = await _httpClient.GetFromJsonAsync<List<VideoDTO>>($"video/search?q={Uri.EscapeDataString(searchTerm)}");
            return videos ?? new List<VideoDTO>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search videos");
            return new List<VideoDTO>();
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

    public string GetDownloadUrl(int videoId, string? quality = "original")
    {
        var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "https://localhost:7250/api";
        return $"{baseUrl}/video/{videoId}/download?quality={quality}";
    }
}
