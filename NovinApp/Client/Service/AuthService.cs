using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using NovinApp.Client.Auth;
using NovinApp.Shared.Login;

namespace NovinApp.Client.Service
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly JwtAuthenticationStateProvider _authProvider;

        public AuthService(
            HttpClient httpClient,
            ILocalStorageService localStorage,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authProvider = (JwtAuthenticationStateProvider)authenticationStateProvider;
        }

        public async Task<bool> LoginAsync(UserInfo userInfo)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/Login", userInfo);

                if (!response.IsSuccessStatusCode)
                    return false;

                var userToken = await response.Content.ReadFromJsonAsync<UserToken>();

                if (userToken == null || string.IsNullOrWhiteSpace(userToken.token))
                    return false;

                // ذخیره JWT در LocalStorage
                await _localStorage.SetItemAsync("authToken", userToken.token);

                // ست Authorization Header به عنوان پشتیبان
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", userToken.token);

                // اطلاع به AuthenticationState
                _authProvider.NotifyUserAuthentication(userToken.token);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during LoginAsync: {ex.Message}");
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _localStorage.RemoveItemAsync("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = null;
                _authProvider.NotifyUserLogout();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during LogoutAsync: {ex.Message}");
            }
        }
    }
}
