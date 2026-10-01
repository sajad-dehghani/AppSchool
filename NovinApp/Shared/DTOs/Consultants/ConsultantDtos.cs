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
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateConsultantDto
    {
        [Required(ErrorMessage = "نام و نام خانوادگی الزامی است.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "کد ملی الزامی است.")]
        [MaxLength(20, ErrorMessage = "حداکثر ۲۰ کاراکتر")]
        public string NationalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "کلمه عبور الزامی است.")]
        [MinLength(4, ErrorMessage = "حداقل ۴ کاراکتر")]
        public string Password { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Specialty { get; set; }
        public string? Bio { get; set; }
        public string? Gender { get; set; } = "مرد";
    }

    public class UpdateConsultantDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "نام الزامی است.")]
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Specialty { get; set; }
        public string? Bio { get; set; }
        public string? Gender { get; set; }
        public bool IsActive { get; set; } = true;
        public string? NewPassword { get; set; }
    }
}
