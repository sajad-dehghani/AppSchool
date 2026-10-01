using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovinApp.Shared
{
    public static class PsychologyTestTypes
    {
        public const string Holland = "Holland";
        public const string Gardner = "Gardner";
        public const string NEO = "NEO";
        public const string Cattell = "Cattell";
        public const string Clifton = "Clifton";
    }

    [Table("PsychologyTestResults")]
    public class PsychologyTestResult
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string StudentNationalCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string StudentName { get; set; } = string.Empty;

        public int? Id_Moshaver { get; set; }

        [Required]
        [MaxLength(50)]
        public string TestType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TestTitle { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string? PrimaryScore { get; set; } // مثال: RIA یا منطقی-ریاضی یا تیپ شخصیتی

        [Required]
        public string ScoresJson { get; set; } = "[]"; // لیست ابعاد و امتیازات

        [Required]
        public string AnalysisText { get; set; } = string.Empty; // متن تحلیل استاندارد

        public string? HighSchoolBranchesJson { get; set; } // شاخه‌های تحصیلی پیشنهادی پایه نهم

        public string? UniversityMajorsJson { get; set; } // رشته‌های پیشنهادی دانشگاهی و کنکور

        public string? SuitableCareersJson { get; set; } // مشاغل و حرفه‌های پیشنهادی

        public string? AiRecommendation { get; set; } // نظریه نهایی و ارزیابی هوش مصنوعی

        public bool IsAiGenerated { get; set; } = false;
    }

    public class TestQuestion
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string CategoryKey { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int? SelectedScore { get; set; } = null;
        public bool IsReverseKeyed { get; set; } = false; // برای سوالات نمره‌گذاری معکوس در نئو
    }

    public class TestScoreDimension
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string PersianDesc { get; set; } = string.Empty;
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double Percentage { get; set; }
        public string LevelTitle { get; set; } = string.Empty; // ضعیف، متوسط، خوب، بسیار قوی
        public string ColorHex { get; set; } = "#1976d2";
        public string Icon { get; set; } = string.Empty;
    }

    public class TestAnalysisReport
    {
        public string TestType { get; set; } = string.Empty;
        public string TestTitle { get; set; } = string.Empty;
        public string PrimaryCode { get; set; } = string.Empty;
        public List<TestScoreDimension> Dimensions { get; set; } = new();
        public string SummaryAnalysis { get; set; } = string.Empty;
        public List<string> HighSchoolBranches { get; set; } = new();
        public List<string> UniversityMajors { get; set; } = new();
        public List<string> SuitableCareers { get; set; } = new();
        public string? AiGuidance { get; set; }
    }

    public class CareerDomainEvaluation
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> SuggestedJobs { get; set; } = new();
        public int MatchPercentage { get; set; }
        public string ColorHex { get; set; } = "#1976d2";
        public string Icon { get; set; } = "School";
    }

    public class HighSchoolBranchStatus
    {
        public string BranchName { get; set; } = string.Empty;
        public string BranchKey { get; set; } = string.Empty;
        public string Suitability { get; set; } = "متوسط"; // خیلی زیاد، زیاد، متوسط، کم، خیلی کم
        public string SuitabilityColor { get; set; } = "#fbc02d";
        public string HeaderColor { get; set; } = "#1976d2";
        public string Icon { get; set; } = "School";
        public List<string> SuitableMajors { get; set; } = new();
        public List<string> UnsuitableMajors { get; set; } = new();
    }

    public class MajorSuitabilityItem
    {
        public string MajorName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public int Stars { get; set; } = 5; // out of 10
        public int DistanceArrows { get; set; } = 6; // ↓ count
    }

    public class ComprehensiveCareerReport
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentNationalCode { get; set; } = string.Empty;
        public string CounselorName { get; set; } = string.Empty;
        public string InstitutionName { get; set; } = "موسسه آموزشی و هدایت تحصیلی نوین";
        public string TestDatePersian { get; set; } = string.Empty;
        public string TestTitle { get; set; } = string.Empty;
        public string PersonalitySummary { get; set; } = string.Empty;
        public List<CareerDomainEvaluation> Domains { get; set; } = new();
        public List<HighSchoolBranchStatus> BranchStatuses { get; set; } = new();
        public List<MajorSuitabilityItem> RecommendedMajors { get; set; } = new();
        public List<MajorSuitabilityItem> DistantMajors { get; set; } = new();
        public string? AiFinalGuidance { get; set; }
    }
}

