using System.Net.Http.Json;
using BlazorCMS.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorCMS.Admin.Services
{
    public class AdminBlogService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AdminBlogService> _logger;

        public AdminBlogService(HttpClient http, ILogger<AdminBlogService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<BlogPostDTO>> GetAllBlogsAsync()
        {
            try
            {
                _logger.LogInformation("📡 Fetching all blogs...");
                var blogs = await _http.GetFromJsonAsync<List<BlogPostDTO>>("https://localhost:7250/api/blog");
                _logger.LogInformation("✅ {Count} blogs retrieved.", blogs?.Count ?? 0);
                return blogs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to fetch blogs.");
                return new List<BlogPostDTO>();
            }
        }

        public async Task<bool> DeleteBlogAsync(int id)
        {
            var response = await _http.DeleteAsync($"blog/{id}");
            return response.IsSuccessStatusCode;
        }
        public async Task<List<BlogPostDTO>> GetAllBlogs() =>
         await _http.GetFromJsonAsync<List<BlogPostDTO>>("https://localhost:7250/api/blog");

        public async Task<BlogPostDTO> GetBlogById(int id)
        {
            try
            {
                _logger.LogInformation("📡 Fetching blog with ID {Id}...", id);
                return await _http.GetFromJsonAsync<BlogPostDTO>($"https://localhost:7250/api/blog/details/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to fetch blog with ID {Id}.", id);
                return null;
            }
        }

        public async Task CreateBlog(BlogPostDTO blog)
        {
            try
            {
                _logger.LogInformation("📝 Creating new blog: {Title}", blog.Title);
                await _http.PostAsJsonAsync("https://localhost:7250/api/blog", blog);
                _logger.LogInformation("✅ Blog created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to create blog: {Title}", blog.Title);
            }
        }

        public async Task UpdateBlog(BlogPostDTO blog)
        {
            try
            {
                _logger.LogInformation("✏ Updating blog: {Title}", blog.Title);
                await _http.PutAsJsonAsync($"https://localhost:7250/api/blog/{blog.Id}", blog);
                _logger.LogInformation("✅ Blog updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to update blog: {Title}", blog.Title);
            }
        }

        public async Task DeleteBlog(int id)
        {
            try
            {
                _logger.LogInformation("🗑 Deleting blog with ID {Id}...", id);
                await _http.DeleteAsync($"https://localhost:7250/api/blog/{id}");
                _logger.LogInformation("✅ Blog deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to delete blog with ID {Id}.", id);
            }
        }
    }
}
