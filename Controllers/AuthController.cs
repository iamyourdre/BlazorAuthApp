using BlazorAuthApp.Entities;
using BlazorAuthApp.Models;
using BlazorAuthApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorAuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDTO request)
        {
            var user = await authService.RegisterAsync(request);
            if (user == null)
                return BadRequest("User already exists");
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDTO>> Login(UserDTO request)
        {
            var token = await authService.LoginAsync(request);
            if (token == null)
                return Unauthorized("Invalid username or password");
            return Ok(token);
        }

        [Authorize]
        [HttpGet("authenticated")]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            if (User.Identity?.IsAuthenticated == true)
                return Ok("You are authenticated");

            return Unauthorized("You are not authenticated");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyEndpoint()
        {
            if (User.IsInRole("Admin"))
                return Ok("You are an admin");
            return Forbid("You are not authorized to access this endpoint");
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDTO>> RefreshToken(RefreshTokenRequestDTO request)
        {
            var token = await authService.RefreshTokenAsync(request);
            if (token == null)
                return Unauthorized("Invalid refresh token");
            return Ok(token);
        }


    }

}
