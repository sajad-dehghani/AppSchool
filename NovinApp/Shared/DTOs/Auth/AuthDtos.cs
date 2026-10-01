using System.ComponentModel.DataAnnotations;

namespace NovinApp.Shared.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "نام کاربری الزامی است.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "کلمه عبور الزامی است.")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserProfileDto? User { get; set; }
    }

    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string NationalCode { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string Role { get; set; } = string.Empty;
        public string RolePersian { get; set; } = string.Empty;
        public int? StudentId { get; set; }
        public int? ConsultantId { get; set; }
        public int? SchoolId { get; set; }
        public string? SchoolName { get; set; }
        public string? ConsultantName { get; set; }
        public bool IsActive { get; set; }
    }

    public class RefreshTokenRequestDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
