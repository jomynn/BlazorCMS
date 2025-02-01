using System.Net.Http.Json;
using BlazorCMS.Shared.DTOs;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorCMS.Client.Services
{
    public class ClientPageService
    {
        private readonly HttpClient _http;

        public ClientPageService(HttpClient http)
        {
            _http = http;
        }

        // Fetch all CMS pages
        public async Task<List<PageDTO>> GetAllPagesAsync()
        {
            return await _http.GetFromJsonAsync<List<PageDTO>>("pages");
        }

        // Fetch a single CMS page by ID
        public async Task<PageDTO> GetPageByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<PageDTO>($"pages/{id}");
        }

        // Create a new CMS page
        public async Task<bool> CreatePageAsync(PageDTO page)
        {
            var response = await _http.PostAsJsonAsync("pages", page);
            return response.IsSuccessStatusCode;
        }

        // Update an existing CMS page
        public async Task<bool> UpdatePageAsync(int id, PageDTO page)
        {
            var response = await _http.PutAsJsonAsync($"pages/{id}", page);
            return response.IsSuccessStatusCode;
        }

        // Delete a CMS page by ID
        public async Task<bool> DeletePageAsync(int id)
        {
            var response = await _http.DeleteAsync($"pages/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
