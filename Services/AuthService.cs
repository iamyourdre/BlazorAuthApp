using BlazorAuthApp.Data;
using BlazorAuthApp.Entities;
using BlazorAuthApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
        public Task<string?> LoginAsync(UserDTO request)
        {
            throw new NotImplementedException();
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
    }
}
