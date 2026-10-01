using System.Security.Claims;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using NovinApp.Shared;
using NovinApp.Shared.Constants;

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

        // Helper properties for role checks
        public bool IsSystemAdmin => IsRoleMatch(UserRoles.SystemAdmin);
        public bool IsSchoolManager => IsRoleMatch(UserRoles.SchoolManager);
        public bool IsConsultant => IsRoleMatch(UserRoles.Consultant);
        public bool IsStudent => IsRoleMatch(UserRoles.Student);

        private bool IsRoleMatch(string role)
        {
            if (CurrentUser?.Rool == null) return false;
            return UserRoles.NormalizeRole(CurrentUser.Rool) == role;
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

                // اگر User از JWT claims بارگذاری نشد، سعی می‌کنیم از claims نقش را بگیریم
                if (CurrentUser != null)
                {
                    // هماهنگ‌سازی نقش از JWT claim با مدل User
                    if (string.IsNullOrEmpty(CurrentUser.Rool))
                    {
                        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value
                            ?? user.FindFirst("Role")?.Value;
                        if (!string.IsNullOrEmpty(roleClaim))
                            CurrentUser.Rool = roleClaim;
                    }

                    IsLoaded = true;
                    NotifyStateChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user profile: {ex.Message}");

                // Fallback: Create a minimal user from JWT claims
                try
                {
                    var fullName = user.FindFirst("FullName")?.Value ?? user.FindFirst(ClaimTypes.Name)?.Value ?? "";
                    var nationalCode = user.FindFirst("NationalCode")?.Value ?? "";
                    var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst("Role")?.Value ?? UserRoles.Student;

                    if (!string.IsNullOrEmpty(fullName) || !string.IsNullOrEmpty(nationalCode))
                    {
                        CurrentUser = new User
                        {
                            Id = int.TryParse(userId, out var id) ? id : 0,
                            fname = fullName,
                            code_meli = nationalCode,
                            Rool = roleClaim,
                            gender = "نامشخص",
                            mobile = "",
                            pass = "",
                            active = true
                        };
                        IsLoaded = true;
                        NotifyStateChanged();
                    }
                }
                catch { }
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
