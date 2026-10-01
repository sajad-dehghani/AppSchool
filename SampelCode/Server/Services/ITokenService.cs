using System.Security.Claims;
using System.Threading.Tasks;
using NovinApp.Server.Models;
using NovinApp.Shared.DTOs.Auth;
using NovinApp.Shared.Entities;

namespace NovinApp.Server.Services
{
    public interface ITokenService
    {
        Task<LoginResponseDto> CreateTokenResponseAsync(ApplicationUser user);
        Task<LoginResponseDto?> RefreshTokenAsync(string accessToken, string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken, int? userId = null);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
