using System.Net.Http.Json;
using BlazorCMS.Shared.DTOs;
using System.Threading.Tasks;

namespace BlazorCMS.Client.Services
{
    public class ClientAuthService
    {
        private readonly HttpClient _http;

        public ClientAuthService(HttpClient http)
        {
            _http = http;
        }

        // Login API Call
        public async Task<bool> LoginAsync(LoginDTO login)
        {
            var response = await _http.PostAsJsonAsync("auth/login", login);
            return response.IsSuccessStatusCode;
        }

        // Register API Call (Added this method)
        public async Task<bool> RegisterAsync(RegisterDTO register)
        {
            var response = await _http.PostAsJsonAsync("auth/register", register);
            return response.IsSuccessStatusCode;
        }
    }
}
