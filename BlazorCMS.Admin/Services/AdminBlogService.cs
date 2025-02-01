using System.Net.Http.Json;
using BlazorCMS.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorCMS.Admin.Services
{
    public class AdminBlogService
    {
        private readonly HttpClient _http;

        public AdminBlogService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<BlogPostDTO>> GetAllBlogsAsync()
        {
            return await _http.GetFromJsonAsync<List<BlogPostDTO>>("blog");
        }

        public async Task<bool> DeleteBlogAsync(int id)
        {
            var response = await _http.DeleteAsync($"blog/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
