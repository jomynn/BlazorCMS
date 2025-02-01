using Microsoft.AspNetCore.Identity;
using BlazorCMS.Shared.DTOs;
using BlazorCMS.Infrastructure.Authentication;
using BlazorCMS.Shared.Models;
using System.Threading.Tasks;

namespace BlazorCMS.API.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtTokenService _jwtService;

        public AuthService(UserManager<ApplicationUser> userManager, JwtTokenService jwtService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
        }

        public async Task<(bool Success, string Message, string Token)> LoginAsync(LoginDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return (false, "Invalid credentials", null);
            }

            var token = _jwtService.GenerateToken(user.Id, user.Email, "User");
            return (true, "Login successful", token);
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDTO model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return (false, "Registration failed");
            }

            return (true, "Registration successful");
        }
    }
}
