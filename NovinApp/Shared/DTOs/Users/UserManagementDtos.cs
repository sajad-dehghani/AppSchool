namespace NovinApp.Shared.DTOs.Users
{
    public class UserFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public int? SchoolId { get; set; }
        public int? ConsultantId { get; set; }
        public bool? IsActive { get; set; }
        public bool? WithoutConsultantOnly { get; set; }
        public bool? WithoutSchoolOnly { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = true;
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string NationalCode { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string Role { get; set; } = string.Empty;
        public string RolePersian { get; set; } = string.Empty;
        public int? SchoolId { get; set; }
        public string? SchoolName { get; set; }
        public int? ConsultantId { get; set; }
        public string? ConsultantName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalConsultants { get; set; }
        public int TotalSchoolManagers { get; set; }
        public int TotalAdmins { get; set; }
        public int StudentsWithoutConsultant { get; set; }
        public int StudentsWithoutSchool { get; set; }
        public int InactiveUsers { get; set; }
    }

    public class BatchAssignSchoolDto
    {
        public List<int> UserIds { get; set; } = new();
        public int? SchoolId { get; set; }
    }

    public class BatchAssignConsultantDto
    {
        public List<int> UserIds { get; set; } = new();
        public int? ConsultantId { get; set; }
    }

    public class BatchToggleStatusDto
    {
        public List<int> UserIds { get; set; } = new();
        public bool IsActive { get; set; }
    }

    public class BatchDeleteUsersDto
    {
        public List<int> UserIds { get; set; } = new();
    }

    public class BulkUserImportItemDto
    {
        public string FullName { get; set; } = string.Empty;
        public string NationalCode { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? Role { get; set; }
        public string? SchoolName { get; set; }
        public int? SchoolId { get; set; }
        public string? ConsultantName { get; set; }
        public int? ConsultantId { get; set; }
        public string? GradeLevel { get; set; }
        public string? FieldOfStudy { get; set; }
        public string? StudentCode { get; set; }
    }

    public class BulkImportRequestDto
    {
        public List<BulkUserImportItemDto> Users { get; set; } = new();
        public bool OverwriteExisting { get; set; } = false;
        public int? DefaultSchoolId { get; set; }
        public int? DefaultConsultantId { get; set; }
        public string DefaultRole { get; set; } = "دانش آموز";
    }

    public class BulkImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> ErrorMessages { get; set; } = new();
    }
}
