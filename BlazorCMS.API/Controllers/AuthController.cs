using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BlazorCMS.Shared.DTOs;
using BlazorCMS.Infrastructure.Authentication;
using System.Threading.Tasks;
using BlazorCMS.Data.Models;

namespace BlazorCMS.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtTokenService _jwtService;

        public AuthController(UserManager<ApplicationUser> userManager, JwtTokenService jwtService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(new { message = "Registration failed", errors = result.Errors });

            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new { message = "Invalid credentials" });

            var token = _jwtService.GenerateToken(user.Id, user.Email, "User");
            return Ok(new { token });
        }
    }
}
