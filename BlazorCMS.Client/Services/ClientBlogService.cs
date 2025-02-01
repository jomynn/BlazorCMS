using System.Net.Http.Json;
using BlazorCMS.Shared.DTOs;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorCMS.Client.Services
{
    public class ClientBlogService
    {
        private readonly HttpClient _http;

        public ClientBlogService(HttpClient http)
        {
            _http = http;
        }

        // Fetch all blog posts
        public async Task<List<BlogPostDTO>> GetAllBlogsAsync()
        {
            return await _http.GetFromJsonAsync<List<BlogPostDTO>>("blog");
        }

        // Fetch a single blog post by ID
        public async Task<BlogPostDTO> GetBlogByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<BlogPostDTO>($"blog/{id}");
        }

        // Create a new blog post
        public async Task<bool> CreateBlogAsync(BlogPostDTO blog)
        {
            var response = await _http.PostAsJsonAsync("blog", blog);
            return response.IsSuccessStatusCode;
        }

        // Update an existing blog post
        public async Task<bool> UpdateBlogAsync(int id, BlogPostDTO blog)
        {
            var response = await _http.PutAsJsonAsync($"blog/{id}", blog);
            return response.IsSuccessStatusCode;
        }

        // Delete a blog post by ID
        public async Task<bool> DeleteBlogAsync(int id)
        {
            var response = await _http.DeleteAsync($"blog/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
