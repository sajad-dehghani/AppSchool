using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NovinApp.Shared.Enums;

namespace NovinApp.Shared.Entities
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }

    [Table("Schools")]
    public class School : BaseEntity
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public long? SchoolCode { get; set; }

        [MaxLength(100)]
        public string? RegionName { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        public int? ManagerUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<StudentProfile> Students { get; set; } = new List<StudentProfile>();
    }

    [Table("ConsultantProfiles")]
    public class ConsultantProfile : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [MaxLength(150)]
        public string? Specialty { get; set; } // مثال: مشاور کنکور تجربی، روانشناس تحصیلی

        [MaxLength(500)]
        public string? Bio { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<StudentProfile> Students { get; set; } = new List<StudentProfile>();
        public virtual ICollection<StudyPlan> StudyPlans { get; set; } = new List<StudyPlan>();
    }

    [Table("StudentProfiles")]
    public class StudentProfile : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string NationalCode { get; set; } = string.Empty;

        public long? StudentCode { get; set; }

        [MaxLength(100)]
        public string? FatherName { get; set; }

        [MaxLength(100)]
        public string? GradeLevel { get; set; } // مثال: دوازدهم، دهم

        [MaxLength(100)]
        public string? FieldOfStudy { get; set; } // مثال: علوم تجربی، ریاضی فیزیک، علوم انسانی

        public int? SchoolId { get; set; }
        public virtual School? School { get; set; }

        public int? ConsultantId { get; set; }
        public virtual ConsultantProfile? Consultant { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<StudentExamResult> ExamResults { get; set; } = new List<StudentExamResult>();
        public virtual ICollection<StudyPlan> StudyPlans { get; set; } = new List<StudyPlan>();
        public virtual ICollection<StudyDailyReport> DailyReports { get; set; } = new List<StudyDailyReport>();
    }

    [Table("Exams")]
    public class Exam : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty; // مثال: آزمون مرحله ۵ گزینه دو

        [MaxLength(100)]
        public string? Provider { get; set; } // گزینه دو، قلم‌چی، سنجش، گاج، نوین

        public int StageNumber { get; set; } = 1;

        [MaxLength(50)]
        public string? AcademicYear { get; set; } // ۱۴۰۳-۱۴۰۴

        public DateTime? ExamDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public virtual ICollection<StudentExamResult> Results { get; set; } = new List<StudentExamResult>();
    }

    [Table("Subjects")]
    public class Subject : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty; // زیست‌شناسی، ریاضی، شیمی، فیزیک

        [MaxLength(50)]
        public string? Code { get; set; }

        [MaxLength(100)]
        public string? FieldOfStudy { get; set; } // تجربی، ریاضی، انسانی، عمومی

        public int DisplayOrder { get; set; } = 0;
    }

    [Table("StudentExamResults")]
    public class StudentExamResult : BaseEntity
    {
        public int StudentProfileId { get; set; }
        public virtual StudentProfile StudentProfile { get; set; } = null!;

        public int ExamId { get; set; }
        public virtual Exam Exam { get; set; } = null!;

        public float TotalPercent { get; set; }
        public float TotalTaraz { get; set; }
        public float TotalPercentAverage { get; set; }
        public float TotalMaxTaraz { get; set; }

        public int RankInSchool { get; set; }
        public int RankInRegion { get; set; }
        public int RankInTotal { get; set; }

        [MaxLength(500)]
        public string? StrengthsSummary { get; set; }

        [MaxLength(500)]
        public string? WeaknessesSummary { get; set; }

        public int? ImportSessionId { get; set; }

        public virtual ICollection<StudentExamSubjectResult> SubjectResults { get; set; } = new List<StudentExamSubjectResult>();
    }

    [Table("StudentExamSubjectResults")]
    public class StudentExamSubjectResult : BaseEntity
    {
        public int StudentExamResultId { get; set; }
        public virtual StudentExamResult StudentExamResult { get; set; } = null!;

        public int? SubjectId { get; set; }
        public virtual Subject? Subject { get; set; }

        [Required]
        [MaxLength(100)]
        public string SubjectName { get; set; } = string.Empty;

        public float Percent { get; set; }
        public float Taraz { get; set; }
        public float PercentAverage { get; set; }
        public float MaxTaraz { get; set; }

        public int TrueCount { get; set; }
        public int FalseCount { get; set; }
        public int BlankCount { get; set; }

        public int RankInSchool { get; set; }
        public int RankInRegion { get; set; }
        public int RankInTotal { get; set; }

        [MaxLength(100)]
        public string? StatusTitle { get; set; } // عالی، خوب، نیاز به تمرین
    }

    [Table("StudyPlans")]
    public class StudyPlan : BaseEntity
    {
        public int StudentProfileId { get; set; }
        public virtual StudentProfile StudentProfile { get; set; } = null!;

        public int? ConsultantProfileId { get; set; }
        public virtual ConsultantProfile? ConsultantProfile { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty; // برنامه راهبردی مهرماه

        [MaxLength(100)]
        public string TargetBranch { get; set; } = "تجربی";

        public double TargetWeeklyHours { get; set; } = 45;
        public int TargetWeeklyTests { get; set; } = 800;

        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        [MaxLength(1000)]
        public string? ConsultantNote { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<StudyTask> Tasks { get; set; } = new List<StudyTask>();
        public virtual ICollection<StudyDailyReport> DailyReports { get; set; } = new List<StudyDailyReport>();
    }

    [Table("StudyTasks")]
    public class StudyTask : BaseEntity
    {
        public int StudyPlanId { get; set; }
        public virtual StudyPlan StudyPlan { get; set; } = null!;

        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        [MaxLength(100)]
        public string SubjectName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string TopicTitle { get; set; } = string.Empty;

        public double PlannedHours { get; set; }
        public int PlannedTests { get; set; }

        public double ActualHours { get; set; }
        public int ActualTests { get; set; }
        public int CorrectTests { get; set; }
        public int WrongTests { get; set; }

        public StudyTaskStatus Status { get; set; } = StudyTaskStatus.Planned;

        [MaxLength(500)]
        public string? StudentComment { get; set; }
    }

    [Table("StudyDailyReports")]
    public class StudyDailyReport : BaseEntity
    {
        public int? StudyPlanId { get; set; }
        public virtual StudyPlan? StudyPlan { get; set; }

        public int StudentProfileId { get; set; }
        public virtual StudentProfile StudentProfile { get; set; } = null!;

        public DateTime ReportDate { get; set; } = DateTime.Today;

        [Required]
        [MaxLength(100)]
        public string SubjectName { get; set; } = string.Empty;

        public double StudyHours { get; set; }
        public int TestCount { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }

        [MaxLength(300)]
        public string? TopicsCovered { get; set; }

        [MaxLength(500)]
        public string? StudentNote { get; set; }

        [MaxLength(500)]
        public string? CounselorNote { get; set; }

        public int QualityRating { get; set; } = 4; // ۱ تا ۵
        public bool IsApprovedByCounselor { get; set; } = false;
    }

    [Table("ImportSessions")]
    public class ImportSession : BaseEntity
    {
        [Required]
        [MaxLength(250)]
        public string FileName { get; set; } = string.Empty;

        public int? ExamId { get; set; }
        public virtual Exam? Exam { get; set; }

        public int UploadedByUserId { get; set; }

        public ImportStatus Status { get; set; } = ImportStatus.Pending;

        public int TotalRows { get; set; }
        public int SuccessfulRows { get; set; }
        public int FailedRows { get; set; }
        public int WarningRows { get; set; }

        [MaxLength(1000)]
        public string? SummaryMessage { get; set; }

        public virtual ICollection<ImportError> Errors { get; set; } = new List<ImportError>();
    }

    [Table("ImportErrors")]
    public class ImportError : BaseEntity
    {
        public int ImportSessionId { get; set; }
        public virtual ImportSession ImportSession { get; set; } = null!;

        public int RowNumber { get; set; }
        public int? ColumnIndex { get; set; }

        [MaxLength(100)]
        public string? ColumnName { get; set; }

        [MaxLength(200)]
        public string? RawValue { get; set; }

        [MaxLength(100)]
        public string ErrorType { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string ErrorMessage { get; set; } = string.Empty;
    }

    [Table("AuditLogs")]
    public class AuditLog : BaseEntity
    {
        public int? UserId { get; set; }

        [MaxLength(100)]
        public string? UserRole { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty; // Create, Update, Delete, Import, Login, RoleChange

        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? EntityId { get; set; }

        public string? Details { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }
    }

    [Table("RefreshTokens")]
    public class RefreshToken : BaseEntity
    {
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; } = false;

        [MaxLength(200)]
        public string? ReplacedByToken { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
