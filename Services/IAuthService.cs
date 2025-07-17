using BlazorAuthApp.Entities;
using BlazorAuthApp.Models;

namespace BlazorAuthApp.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDTO request);
        Task<TokenResponseDTO?> LoginAsync(UserDTO request);
        Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO request);


    }
}
