using System;
using System.Security.Claims;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using NovinApp.Shared;
using NovinApp.Shared.Constants;
using NovinApp.Shared.DTOs.Auth;
using NovinApp.Shared.DTOs.Common;

namespace NovinApp.Client.Service
{
    public class UserProfileStateService
    {
        private readonly AuthenticationStateProvider _authProvider;
        private readonly HttpClient _httpClient;

        public UserProfileDto? Profile { get; private set; }
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

        public string Role => Profile?.Role ?? CurrentUser?.Rool ?? UserRoles.Student;
        public bool IsSystemAdmin => Role == UserRoles.SystemAdmin;
        public bool IsSchoolManager => Role == UserRoles.SchoolManager;
        public bool IsConsultant => Role == UserRoles.Consultant;
        public bool IsStudent => Role == UserRoles.Student;

        public async Task LoadUserAsync()
        {
            if (IsLoaded)
                return;

            var authState = await _authProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
                return;

            try
            {
                // تلاش اول: دریافت مستقیم پروفایل امن از api/auth/me
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<UserProfileDto>>("api/auth/me");
                if (response != null && response.Success && response.Data != null)
                {
                    Profile = response.Data;
                    CurrentUser = new User
                    {
                        Id = Profile.Id,
                        fname = Profile.FullName,
                        code_meli = Profile.NationalCode,
                        mobile = Profile.PhoneNumber ?? "",
                        pic = Profile.ProfilePictureUrl,
                        Rool = Profile.Role,
                        Id_Moshaver = Profile.ConsultantId,
                        Id_School = Profile.SchoolId,
                        active = Profile.IsActive
                    };

                    IsLoaded = true;
                    NotifyStateChanged();
                    return;
                }
            }
            catch
            {
                // فال‌بک به کلیم‌های توکن در صورت عدم دسترسی موقت به اندپوینت
            }

            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst("Role")?.Value ?? UserRoles.Student;
            var nameClaim = user.FindFirst("FullName")?.Value ?? user.FindFirst(ClaimTypes.Name)?.Value ?? "کاربر";
            var nationalCodeClaim = user.FindFirst("NationalCode")?.Value ?? "";
            var userIdVal = user.FindFirst("UserId")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
            int.TryParse(userIdVal, out var uid);

            Profile = new UserProfileDto
            {
                Id = uid,
                FullName = nameClaim,
                NationalCode = nationalCodeClaim,
                Role = roleClaim,
                RolePersian = UserRoles.GetPersianTitle(roleClaim),
                IsActive = true
            };

            CurrentUser = new User
            {
                Id = uid,
                fname = nameClaim,
                code_meli = nationalCodeClaim,
                Rool = roleClaim,
                active = true
            };

            IsLoaded = true;
            NotifyStateChanged();
        }

        public void Clear()
        {
            Profile = null;
            CurrentUser = null;
            IsLoaded = false;
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
            => OnChange?.Invoke();
    }
}
