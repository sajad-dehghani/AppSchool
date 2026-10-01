using NovinApp.Shared.Login;
using System.Threading.Tasks;

namespace NovinApp.Client.Service
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(UserInfo userInfo);
        Task LogoutAsync();
    }
}