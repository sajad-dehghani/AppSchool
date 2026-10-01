using System;
using System.ComponentModel.DataAnnotations;

namespace NovinApp.Shared.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "کد ملی یا نام کاربری الزامی است")]
        [StringLength(50, ErrorMessage = "فرمت نام کاربری نامعتبر است")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "کلمه عبور الزامی است")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "کلمه عبور باید حداقل ۶ کاراکتر باشد")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserProfileDto User { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class RefreshTokenRequestDto
    {
        [Required]
        public string AccessToken { get; set; } = string.Empty;

        [Required]
        public string RefreshToken { get; set; } = string.Empty;
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
        public bool IsActive { get; set; } = true;
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "کلمه عبور فعلی الزامی است")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "کلمه عبور جدید الزامی است")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "کلمه عبور جدید باید حداقل ۶ کاراکتر باشد")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "تکرار کلمه عبور جدید الزامی است")]
        [Compare(nameof(NewPassword), ErrorMessage = "تکرار کلمه عبور با کلمه عبور جدید مطابقت ندارد")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class RegisterUserDto
    {
        [Required(ErrorMessage = "کد ملی الزامی است")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید ۱۰ رقم باشد")]
        public string NationalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام و نام خانوادگی الزامی است")]
        [StringLength(150, ErrorMessage = "نام وارد شده بیش از حد مجاز است")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "کلمه عبور الزامی است")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "کلمه عبور باید حداقل ۶ کاراکتر باشد")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "نقش کاربر الزامی است")]
        public string Role { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public int? SchoolId { get; set; }
        public int? ConsultantId { get; set; }
        public string? GradeLevel { get; set; }
        public string? FieldOfStudy { get; set; }
        public string? FatherName { get; set; }
    }
}
