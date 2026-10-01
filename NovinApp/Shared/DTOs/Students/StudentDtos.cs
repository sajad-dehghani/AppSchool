using System.ComponentModel.DataAnnotations;

namespace NovinApp.Shared.DTOs.Students
{
    public class StudentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string NationalCode { get; set; } = string.Empty;
        public long? StudentCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FatherName { get; set; }
        public string? GradeLevel { get; set; }
        public string? FieldOfStudy { get; set; }
        public int? SchoolId { get; set; }
        public string? SchoolName { get; set; }
        public int? ConsultantId { get; set; }
        public string? ConsultantName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateStudentDto
    {
        [Required(ErrorMessage = "نام و نام خانوادگی الزامی است.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "کد ملی الزامی است.")]
        [MaxLength(20, ErrorMessage = "حداکثر ۲۰ کاراکتر")]
        public string NationalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "کلمه عبور الزامی است.")]
        [MinLength(4, ErrorMessage = "حداقل ۴ کاراکتر")]
        public string Password { get; set; } = string.Empty;

        public long? StudentCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FatherName { get; set; }
        public string? GradeLevel { get; set; } = "پایه دوازدهم";
        public string? FieldOfStudy { get; set; } = "علوم تجربی";
        public string? Gender { get; set; } = "مرد";
        public int? SchoolId { get; set; }
        public int? ConsultantId { get; set; }
    }

    public class UpdateStudentDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "نام الزامی است.")]
        public string FullName { get; set; } = string.Empty;
        public long? StudentCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FatherName { get; set; }
        public string? GradeLevel { get; set; }
        public string? FieldOfStudy { get; set; }
        public string? Gender { get; set; }
        public int? SchoolId { get; set; }
        public int? ConsultantId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? NewPassword { get; set; }
    }

    public class AssignStudentDto
    {
        public int StudentId { get; set; }
        public int? SchoolId { get; set; }
        public int? ConsultantId { get; set; }
    }

    public class BulkAssignStudentsDto
    {
        public List<int> StudentIds { get; set; } = new();
        public int? SchoolId { get; set; }
        public int? ConsultantId { get; set; }
        public bool UpdateSchool { get; set; } = false;
        public bool UpdateConsultant { get; set; } = false;
    }

    public class BulkDeleteStudentsDto
    {
        public List<int> StudentIds { get; set; } = new();
    }
}

