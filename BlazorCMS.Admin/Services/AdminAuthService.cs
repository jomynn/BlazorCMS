using System.Net.Http.Json;
using BlazorCMS.Shared.DTOs;
using System.Threading.Tasks;

namespace BlazorCMS.Admin.Services
{
    public class AdminAuthService
    {
        private readonly HttpClient _http;

        public AdminAuthService(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> LoginAsync(LoginDTO login)
        {
            var response = await _http.PostAsJsonAsync("auth/login", login);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RegisterAsync(RegisterDTO register)
        {
            var response = await _http.PostAsJsonAsync("auth/register", register);
            return response.IsSuccessStatusCode;
        }
    }
}
