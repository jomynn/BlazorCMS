using BlazorCMS.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorCMS.Admin.Services
{
    public class AdminAuthService
    {
        private readonly HttpClient _http;
        private readonly UILoggerService _uiLogger;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AdminAuthService(HttpClient http, UILoggerService uiLogger, AuthenticationStateProvider authStateProvider)
        {
            _http = http;
            _uiLogger = uiLogger;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> LoginAsync(LoginDTO loginDto)
        {
            try
            {
                _uiLogger.Log("📡 Sending login request...");
                var response = await _http.PostAsJsonAsync("https://localhost:7250/api/auth/login", loginDto);

                if (!response.IsSuccessStatusCode)
                {
                    _uiLogger.Log($"❌ Login failed: {response.StatusCode}");
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
                if (result == null || string.IsNullOrWhiteSpace(result.Token))
                {
                    _uiLogger.Log("❌ Login response is empty.");
                    return false;
                }

                _uiLogger.Log("✅ Login successful. Token received.");
                await ((CustomAuthStateProvider)_authStateProvider).SetAuthToken(result.Token);
                return true;
            }
            catch (Exception ex)
            {
                _uiLogger.Log($"{ex} ❌ Error during login.");
                return false;
            }
        }
        public async Task LogoutAsync()
        {
            _uiLogger.Log("🚪 Logging out...");
            await ((CustomAuthStateProvider)_authStateProvider).ClearAuthToken();
        }
        public async Task<bool> RegisterAsync(RegisterDTO register)
        {
            var response = await _http.PostAsJsonAsync("auth/register", register);
            return response.IsSuccessStatusCode;
        }
    }
}
