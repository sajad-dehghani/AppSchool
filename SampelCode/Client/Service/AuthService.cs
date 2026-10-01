using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using NovinApp.Client.Auth;
using NovinApp.Shared.DTOs.Auth;
using NovinApp.Shared.DTOs.Common;
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
                var request = new LoginRequestDto
                {
                    Username = userInfo.Username,
                    Password = userInfo.Password
                };

                var result = await LoginAsync(request);
                return result.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during LoginAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();

                if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                {
                    // ذخیره AccessToken و RefreshToken در LocalStorage
                    await _localStorage.SetItemAsync("authToken", apiResponse.Data.AccessToken);
                    await _localStorage.SetItemAsync("refreshToken", apiResponse.Data.RefreshToken);

                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", apiResponse.Data.AccessToken);

                    _authProvider.NotifyUserAuthentication(apiResponse.Data.AccessToken);

                    return apiResponse;
                }

                return apiResponse ?? ApiResponse<LoginResponseDto>.Fail("خطا در برقراری ارتباط با سرور.");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponseDto>.Fail($"خطا در ورود به سیستم: {ex.Message}");
            }
        }

        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                var accessToken = await _localStorage.GetItemAsync<string>("authToken");
                var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

                if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
                    return false;

                var refreshDto = new RefreshTokenRequestDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };

                var response = await _httpClient.PostAsJsonAsync("api/auth/refresh-token", refreshDto);
                if (!response.IsSuccessStatusCode)
                    return false;

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
                if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                {
                    await _localStorage.SetItemAsync("authToken", apiResponse.Data.AccessToken);
                    await _localStorage.SetItemAsync("refreshToken", apiResponse.Data.RefreshToken);

                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", apiResponse.Data.AccessToken);

                    _authProvider.NotifyUserAuthentication(apiResponse.Data.AccessToken);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    try
                    {
                        await _httpClient.PostAsJsonAsync("api/auth/revoke-token", refreshToken);
                    }
                    catch { }
                }

                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("refreshToken");
                _httpClient.DefaultRequestHeaders.Authorization = null;
                _authProvider.NotifyUserLogout();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during LogoutAsync: {ex.Message}");
            }
        }

        public async Task<UserProfileDto?> GetCurrentUserProfileAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<UserProfileDto>>("api/auth/me");
                return response?.Data;
            }
            catch
            {
                return null;
            }
        }
    }
}
