using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovinApp.Shared.DTOs.Exams
{
    public class ExamDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Provider { get; set; }
        public int StageNumber { get; set; } = 1;
        public string? AcademicYear { get; set; }
        public DateTime? ExamDate { get; set; }
        public string? Description { get; set; }
        public int ParticipantsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateExamDto
    {
        [Required(ErrorMessage = "عنوان آزمون الزامی است.")]
        [MaxLength(200, ErrorMessage = "حداکثر طول عنوان آزمون ۲۰۰ کاراکتر است.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Provider { get; set; }

        public int StageNumber { get; set; } = 1;

        [MaxLength(50)]
        public string? AcademicYear { get; set; }

        public DateTime? ExamDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateExamDto : CreateExamDto
    {
        public int Id { get; set; }
    }

    public class SubjectReportCardDto
    {
        public int? SubjectId { get; set; }
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
        public string? StatusTitle { get; set; } // عالی، خوب، نیاز به تمرین
    }

    public class ReportCardDto
    {
        // اطلاعات آزمون
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string? Provider { get; set; }
        public int StageNumber { get; set; }
        public DateTime? ExamDate { get; set; }

        // مشخصات داوطلب
        public int StudentProfileId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string NationalCode { get; set; } = string.Empty;
        public string? StudentNumber { get; set; }
        public string? SchoolName { get; set; }
        public string? GradeLevel { get; set; }
        public string? Major { get; set; }
        public string? ConsultantName { get; set; }

        // نتایج کل
        public float TotalPercent { get; set; }
        public float TotalTaraz { get; set; }
        public float TotalPercentAverage { get; set; }
        public float TotalMaxTaraz { get; set; }
        public int RankInSchool { get; set; }
        public int RankInRegion { get; set; }
        public int RankInTotal { get; set; }
        public int TotalParticipantsSchool { get; set; }
        public int TotalParticipantsOverall { get; set; }

        // تحلیل هوشمند نقاط قوت و ضعف
        public string? StrengthsSummary { get; set; }
        public string? WeaknessesSummary { get; set; }
        public List<string> StrongSubjects { get; set; } = new();
        public List<string> WeakSubjects { get; set; } = new();

        // ریز نمرات دروس
        public List<SubjectReportCardDto> Subjects { get; set; } = new();

        // روند پیشرفت در آزمون‌های پیشین
        public List<StudentExamHistoryItemDto> ExamHistory { get; set; } = new();
    }

    public class StudentExamHistoryItemDto
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public DateTime? ExamDate { get; set; }
        public int StageNumber { get; set; }
        public float TotalPercent { get; set; }
        public float TotalTaraz { get; set; }
        public int RankInSchool { get; set; }
        public int RankInTotal { get; set; }
    }

    public class ExamAnalyticsDto
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public int TotalParticipants { get; set; }
        public float AveragePercent { get; set; }
        public float MaxPercent { get; set; }
        public float MinPercent { get; set; }
        public float AverageTaraz { get; set; }
        public float MaxTaraz { get; set; }
        public List<SubjectAnalyticsDto> Subjects { get; set; } = new();
        public List<TopStudentDto> TopStudents { get; set; } = new();
    }

    public class SubjectAnalyticsDto
    {
        public string SubjectName { get; set; } = string.Empty;
        public float AveragePercent { get; set; }
        public float MaxPercent { get; set; }
        public float MinPercent { get; set; }
    }

    public class TopStudentDto
    {
        public int StudentProfileId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string NationalCode { get; set; } = string.Empty;
        public string? SchoolName { get; set; }
        public float TotalPercent { get; set; }
        public float TotalTaraz { get; set; }
        public int RankInTotal { get; set; }
    }

    public class ExamStudentResultItemDto
    {
        public int ResultId { get; set; }
        public int StudentProfileId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string NationalCode { get; set; } = string.Empty;
        public string? SchoolName { get; set; }
        public float TotalPercent { get; set; }
        public float TotalTaraz { get; set; }
        public int RankInSchool { get; set; }
        public int RankInTotal { get; set; }
        public int SubjectCount { get; set; }
    }
}
