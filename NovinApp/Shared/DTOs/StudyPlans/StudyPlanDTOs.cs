using System;
using System.ComponentModel.DataAnnotations;
using NovinApp.Shared.Enums;

namespace NovinApp.Shared.DTOs.StudyPlans
{
    public class StudyPlanItemDto
    {
        public int Id { get; set; }
        public int ConsultantUserId { get; set; }
        public string? ConsultantName { get; set; }
        public int StudentUserId { get; set; }
        public string? StudentName { get; set; }
        public int? SchoolId { get; set; }

        public string PersianDate { get; set; } = string.Empty;
        public DateTime GregorianDate { get; set; }
        public string StartTime { get; set; } = "08:00";
        public int DurationMinutes { get; set; } = 60;

        public string ActivityType { get; set; } = ActivityTypes.Study;
        public int? TestCount { get; set; }
        public string? TestType { get; set; }
        public string? GradeLevel { get; set; }
        public string? FieldOfStudy { get; set; }
        public string? Subject { get; set; }
        public string? Chapter { get; set; }
        public string? Topic { get; set; }
        public string? ConsultantNote { get; set; }
        public string? ConsultantAttachmentUrl { get; set; }
        public string? ConsultantAttachmentName { get; set; }

        public bool IsCounselingSession { get; set; }
        public string? CounselingType { get; set; }
        public string? MeetingLink { get; set; }
        public StudyTaskStatus Status { get; set; }

        // بازخورد دانش‌آموز
        public bool IsFeedbackSubmitted { get; set; }
        public DateTime? FeedbackSubmittedAt { get; set; }
        public string? StudentStatus { get; set; }
        public int? QualityRating { get; set; }
        public string? QualityText { get; set; }
        public int? StudyPercentage { get; set; }
        public int? CorrectTests { get; set; }
        public int? WrongTests { get; set; }
        public int? UnansweredTests { get; set; }
        public int? ActualDurationMinutes { get; set; }
        public string? StudentNote { get; set; }
        public string? StudentAttachmentUrl { get; set; }
        public string? StudentAttachmentName { get; set; }
        public string? TestSource { get; set; }

        // فیلدهای محاسباتی
        public double TestAccuracyPercentage => (TestCount > 0 && CorrectTests.HasValue) 
            ? Math.Round((double)CorrectTests.Value / Math.Max(1, (CorrectTests.Value + (WrongTests ?? 0) + (UnansweredTests ?? 0))) * 100, 1) 
            : 0;
    }

    public class StudyPlanCreateUpdateDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "انتخاب دانش‌آموز الزامی است")]
        public int StudentUserId { get; set; }
        public string? StudentName { get; set; }

        [Required(ErrorMessage = "تاریخ برنامه الزامی است")]
        public string PersianDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "ساعت شروع الزامی است")]
        public string StartTime { get; set; } = "08:00";

        [Range(5, 720, ErrorMessage = "مدت زمان باید بین ۵ تا ۷۲۰ دقیقه باشد")]
        public int DurationMinutes { get; set; } = 60;

        [Required(ErrorMessage = "نوع فعالیت الزامی است")]
        public string ActivityType { get; set; } = ActivityTypes.Study;

        public int? TestCount { get; set; }
        public string? TestType { get; set; }
        public string? GradeLevel { get; set; }
        public string? FieldOfStudy { get; set; }
        public string? Subject { get; set; }
        public string? Chapter { get; set; }
        public string? Topic { get; set; }
        public string? ConsultantNote { get; set; }
        public string? ConsultantAttachmentUrl { get; set; }
        public string? ConsultantAttachmentName { get; set; }

        public bool IsCounselingSession { get; set; }
        public string? CounselingType { get; set; }
        public string? MeetingLink { get; set; }
    }

    public class StudyFeedbackSubmitDto
    {
        public int PlanId { get; set; }
        [Required(ErrorMessage = "وضعیت انجام را مشخص کنید")]
        public string StudentStatus { get; set; } = "انجام شد"; // انجام شد، ناقص، انجام نشد

        [Range(1, 5, ErrorMessage = "کیفیت باید از ۱ تا ۵ ستاره باشد")]
        public int QualityRating { get; set; } = 5;

        public string? QualityText { get; set; }

        [Range(0, 100, ErrorMessage = "درصد مطالعه باید بین ۰ تا ۱۰۰ باشد")]
        public int StudyPercentage { get; set; } = 100;

        public int? CorrectTests { get; set; }
        public int? WrongTests { get; set; }
        public int? UnansweredTests { get; set; }

        [Range(0, 720, ErrorMessage = "مدت زمان مطالعه واقعی معتبر نیست")]
        public int ActualDurationMinutes { get; set; } = 60;

        public string? StudentNote { get; set; }
        public string? StudentAttachmentUrl { get; set; }
        public string? StudentAttachmentName { get; set; }
        public string? TestSource { get; set; }
    }

    public class StudySubjectTopicDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "پایه تحصیلی الزامی است")]
        public string GradeLevel { get; set; } = string.Empty;

        [Required(ErrorMessage = "رشته تحصیلی الزامی است")]
        public string FieldOfStudy { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام درس الزامی است")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام فصل الزامی است")]
        public string Chapter { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام مبحث الزامی است")]
        public string Topic { get; set; } = string.Empty;

        public string? Description { get; set; }
        public int PriorityOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CounselingRequestDto
    {
        public int Id { get; set; }
        public int StudentUserId { get; set; }
        public string? StudentName { get; set; }
        public int? ConsultantUserId { get; set; }
        public string? ConsultantName { get; set; }

        [Required(ErrorMessage = "نوع مشاوره الزامی است")]
        public string RequestType { get; set; } = "مشاوره آنلاین";

        public string? PreferredDate { get; set; }
        public string? PreferredTime { get; set; }

        [Required(ErrorMessage = "توضیحات و درخواست الزامی است")]
        public string Description { get; set; } = string.Empty;

        public string? MeetingLink { get; set; }
        public string? ConsultantResponse { get; set; }
        public string? ScheduledDate { get; set; }
        public string? ScheduledTime { get; set; }
        public CounselingStatus Status { get; set; } = CounselingStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class StudyPlannerStatsDto
    {
        public int TotalPlans { get; set; }
        public int CompletedPlans { get; set; }
        public int IncompletePlans { get; set; }
        public int SkippedPlans { get; set; }
        public double PlannedTotalHours { get; set; }
        public double ActualCompletedHours { get; set; }
        public int TotalPlannedTests { get; set; }
        public int TotalCorrectTests { get; set; }
        public int TotalWrongTests { get; set; }
        public int TotalUnansweredTests { get; set; }
        public double CompletionRate { get; set; }
        public double TestAccuracyRate { get; set; }
    }
}
