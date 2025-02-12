using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorCMS.Admin.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        private bool _isInitialized = false; // Track if initialization has occurred

        public CustomAuthStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!_isInitialized)
            {
                return new AuthenticationState(_currentUser);
            }

            var token = await GetTokenFromLocalStorage();
            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(_currentUser);
            }

            var user = new ClaimsPrincipal(new ClaimsIdentity(ParseTokenClaims(token), "jwt"));
            return new AuthenticationState(user);
        }

        public async Task SetAuthToken(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
            _isInitialized = true; // Mark initialization complete
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task Logout()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            _isInitialized = false;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
        }

        public async Task InitializeAuthState()
        {
            _isInitialized = true;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        public async Task ClearAuthToken() // ✅ Added ClearAuthToken Method
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            _isInitialized = false;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
        }

        private async Task<string> GetTokenFromLocalStorage()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            }
            catch (JSException)
            {
                return null;
            }
        }

        private IEnumerable<Claim> ParseTokenClaims(string token)
        {
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                return jwt.Claims ?? new List<Claim>();
            }
            catch
            {
                return new List<Claim>();
            }
        }
    }
}
