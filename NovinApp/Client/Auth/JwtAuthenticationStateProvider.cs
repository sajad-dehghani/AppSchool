using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;

namespace NovinApp.Client.Auth
{
    public class JwtAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationState _anonymous =
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        public JwtAuthenticationStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrWhiteSpace(token))
                    return _anonymous;

                var claims = JwtParser.ParseClaimsFromJwt(token).ToList();
                if (!claims.Any())
                    return _anonymous;

                var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return _anonymous;
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            var claims = JwtParser.ParseClaimsFromJwt(token).ToList();
            var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyUserLogout()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
        }
    }
}
