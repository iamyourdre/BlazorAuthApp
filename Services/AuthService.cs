using BlazorAuthApp.Data;
using BlazorAuthApp.Entities;
using BlazorAuthApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BlazorAuthApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(UserDbContext context, IConfiguration configuration)
        {
            this._context = context;
            this._configuration = configuration;
        }

        public async Task<TokenResponseDTO?> LoginAsync(UserDTO request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null)
                return null;
            var passwordCheck = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if(passwordCheck == PasswordVerificationResult.Failed)
                return null;
            // Generate JWT token or any other authentication token here
            return await CreateTokenResponse(user);
        }

        private async Task<TokenResponseDTO> CreateTokenResponse(User user)
        {
            var token = CreateToken(user);
            var refreshToken = await GenerateAndSaveRefreshTokenAsync(user);
            return new TokenResponseDTO
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };
        }

        public async Task<User?> RegisterAsync(UserDTO request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return null;

            var user = new User
            {
                Username = request.Username,
                PasswordHash = new PasswordHasher<User>().HashPassword(new User(), request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }


        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration["AppSettings:Issuer"],
                audience: _configuration["AppSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if (user == null)
                return null;
            return await CreateTokenResponse(user);
        }

        private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.RefreshToken == refreshToken);
            if (user == null || user.RefreshTokenExpiryTime < DateTime.Now)
                return null;
            return user;
        }
    }
}
