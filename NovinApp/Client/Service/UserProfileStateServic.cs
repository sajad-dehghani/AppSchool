using System.Security.Claims;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using NovinApp.Shared;

namespace NovinApp.Client.Service
{
    public class UserProfileStateService
    {
        private readonly AuthenticationStateProvider _authProvider;
        private readonly HttpClient _httpClient;

        public User? CurrentUser { get; private set; }
        public bool IsLoaded { get; private set; }

        public event Action? OnChange;

        public UserProfileStateService(
            AuthenticationStateProvider authProvider,
            HttpClient httpClient)
        {
            _authProvider = authProvider;
            _httpClient = httpClient;
        }

        public async Task LoadUserAsync()
        {
            if (IsLoaded)
                return;

            var authState = await _authProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
                return;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("UserId")?.Value
                ?? user.FindFirst("nameid")?.Value
                ?? user.FindFirst("sub")?.Value
                ?? user.FindFirst(ClaimTypes.Name)?.Value
                ?? user.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
                return;

            try
            {
                CurrentUser = await _httpClient.GetFromJsonAsync<User>($"api/User/{userId}");

                if (CurrentUser != null)
                {
                    IsLoaded = true;
                    NotifyStateChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user profile: {ex.Message}");
            }
        }

        public void Clear()
        {
            CurrentUser = null;
            IsLoaded = false;
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
            => OnChange?.Invoke();
    }
}
