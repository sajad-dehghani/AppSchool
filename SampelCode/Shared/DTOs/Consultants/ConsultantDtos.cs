using System;
using System.ComponentModel.DataAnnotations;

namespace NovinApp.Shared.DTOs.Consultants
{
    public class ConsultantDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string NationalCode { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Specialty { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int StudentsCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateConsultantDto
    {
        [Required(ErrorMessage = "نام و نام خانوادگی الزامی است.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "کد ملی الزامی است.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید ۱۰ رقم باشد.")]
        public string NationalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "کلمه عبور الزامی است.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "کلمه عبور باید حداقل ۶ کاراکتر باشد.")]
        public string Password { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Specialty { get; set; }
        public string? Bio { get; set; }
        public string? Gender { get; set; } = "مرد";
    }

    public class UpdateConsultantDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام و نام خانوادگی الزامی است.")]
        public string FullName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Specialty { get; set; }
        public string? Bio { get; set; }
        public string? Gender { get; set; }
        public bool IsActive { get; set; } = true;
        public string? NewPassword { get; set; }
    }
}
