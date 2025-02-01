using System.Net.Http.Json;
using BlazorCMS.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorCMS.Admin.Services
{
    public class AdminPageService
    {
        private readonly HttpClient _http;

        public AdminPageService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<PageDTO>> GetAllPagesAsync()
        {
            return await _http.GetFromJsonAsync<List<PageDTO>>("pages");
        }

        public async Task<bool> DeletePageAsync(int id)
        {
            var response = await _http.DeleteAsync($"pages/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
