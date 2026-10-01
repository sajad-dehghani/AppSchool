using System.Threading.Tasks;
using NovinApp.Shared.DTOs.Auth;
using NovinApp.Shared.DTOs.Common;
using NovinApp.Shared.Login;

namespace NovinApp.Client.Service
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(UserInfo userInfo);
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<bool> RefreshTokenAsync();
        Task LogoutAsync();
        Task<UserProfileDto?> GetCurrentUserProfileAsync();
    }
}