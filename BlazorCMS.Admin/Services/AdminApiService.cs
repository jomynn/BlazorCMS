using System.Net.Http.Json;
using BlazorCMS.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace BlazorCMS.Admin.Services
{
    /// <summary>
    /// Unified service for all API operations in the admin panel
    /// </summary>
    public class AdminApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AdminApiService> _logger;

        public AdminApiService(HttpClient http, ILogger<AdminApiService> logger)
        {
            _http = http;
            _logger = logger;
        }

        #region Blog API Operations

        public async Task<List<BlogPostDTO>> GetAllBlogsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all blogs...");
                var blogs = await _http.GetFromJsonAsync<List<BlogPostDTO>>("blog");
                _logger.LogInformation("Successfully retrieved {Count} blogs", blogs?.Count ?? 0);
                return blogs ?? new List<BlogPostDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch blogs");
                return new List<BlogPostDTO>();
            }
        }

        public async Task<BlogPostDTO?> GetBlogByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching blog with ID {Id}", id);
                return await _http.GetFromJsonAsync<BlogPostDTO>($"blog/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch blog with ID {Id}", id);
                return null;
            }
        }

        public async Task<bool> CreateBlogAsync(BlogPostDTO blog)
        {
            try
            {
                _logger.LogInformation("Creating new blog: {Title}", blog.Title);
                var response = await _http.PostAsJsonAsync("blog", blog);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Blog created successfully");
                    return true;
                }
                _logger.LogWarning("Failed to create blog. Status: {Status}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create blog: {Title}", blog.Title);
                return false;
            }
        }

        public async Task<bool> UpdateBlogAsync(int id, BlogPostDTO blog)
        {
            try
            {
                _logger.LogInformation("Updating blog ID {Id}: {Title}", id, blog.Title);
                var response = await _http.PutAsJsonAsync($"blog/{id}", blog);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Blog updated successfully");
                    return true;
                }
                _logger.LogWarning("Failed to update blog. Status: {Status}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update blog ID {Id}", id);
                return false;
            }
        }

        public async Task<bool> DeleteBlogAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting blog with ID {Id}", id);
                var response = await _http.DeleteAsync($"blog/{id}");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Blog deleted successfully");
                    return true;
                }
                _logger.LogWarning("Failed to delete blog. Status: {Status}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete blog with ID {Id}", id);
                return false;
            }
        }

        #endregion

        #region Page API Operations

        public async Task<List<PageDTO>> GetAllPagesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all pages...");
                var pages = await _http.GetFromJsonAsync<List<PageDTO>>("pages");
                _logger.LogInformation("Successfully retrieved {Count} pages", pages?.Count ?? 0);
                return pages ?? new List<PageDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch pages");
                return new List<PageDTO>();
            }
        }

        public async Task<PageDTO?> GetPageByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching page with ID {Id}", id);
                return await _http.GetFromJsonAsync<PageDTO>($"pages/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch page with ID {Id}", id);
                return null;
            }
        }

        public async Task<bool> CreatePageAsync(PageDTO page)
        {
            try
            {
                _logger.LogInformation("Creating new page: {Title}", page.Title);
                var response = await _http.PostAsJsonAsync("pages", page);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Page created successfully");
                    return true;
                }
                _logger.LogWarning("Failed to create page. Status: {Status}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create page: {Title}", page.Title);
                return false;
            }
        }

        #endregion

        #region Statistics

        public async Task<DashboardStatsDTO> GetDashboardStatsAsync()
        {
            try
            {
                var blogs = await GetAllBlogsAsync();
                var pages = await GetAllPagesAsync();

                return new DashboardStatsDTO
                {
                    TotalBlogs = blogs.Count,
                    PublishedBlogs = blogs.Count(b => b.IsPublished),
                    DraftBlogs = blogs.Count(b => !b.IsPublished),
                    TotalPages = pages.Count,
                    PublishedPages = pages.Count(p => p.IsPublished),
                    RecentBlogs = blogs.OrderByDescending(b => b.CreatedAt).Take(5).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch dashboard statistics");
                return new DashboardStatsDTO();
            }
        }

        #endregion
    }

    public class DashboardStatsDTO
    {
        public int TotalBlogs { get; set; }
        public int PublishedBlogs { get; set; }
        public int DraftBlogs { get; set; }
        public int TotalPages { get; set; }
        public int PublishedPages { get; set; }
        public List<BlogPostDTO> RecentBlogs { get; set; } = new();
    }
}
