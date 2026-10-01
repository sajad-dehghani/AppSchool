using System.ComponentModel.DataAnnotations;

namespace NovinApp.Shared.DTOs.Schools
{
    public class SchoolDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long? SchoolCode { get; set; }
        public string? RegionName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public int? ManagerUserId { get; set; }
        public string? ManagerFullName { get; set; }
        public int StudentsCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateSchoolDto
    {
        [Required(ErrorMessage = "نام مدرسه الزامی است.")]
        [MaxLength(150, ErrorMessage = "حداکثر ۱۵۰ کاراکتر")]
        public string Name { get; set; } = string.Empty;
        public long? SchoolCode { get; set; }
        [MaxLength(100)] public string? RegionName { get; set; }
        [MaxLength(200)] public string? Address { get; set; }
        [MaxLength(30)] public string? PhoneNumber { get; set; }
        public int? ManagerUserId { get; set; }
    }

    public class UpdateSchoolDto : CreateSchoolDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
