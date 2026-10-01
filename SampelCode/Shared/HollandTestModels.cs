using System;
using System.Collections.Generic;

namespace NovinApp.Shared
{
    public enum HollandCategory
    {
        Realistic = 1,     // واقع‌گرا (فنی و عملی)
        Investigative = 2, // جستجوگر (علمی و پژوهشی)
        Artistic = 3,      // هنری (خلاقیت و طراحی)
        Social = 4,        // اجتماعی (آموزش و مشاوره)
        Enterprising = 5,  // متهور (مدیریت، بازرگانی و رهبری)
        Conventional = 6   // قراردادی (سازمان‌یافته، امور مالی و داده‌ها)
    }

    public class HollandQuestion
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public HollandCategory Category { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int SelectedScore { get; set; } = 3; // 1 = بسیار مخالف/بی‌علاقه تا 5 = بسیار علاقه‌مند
    }

    public class HollandScore
    {
        public HollandCategory Category { get; set; }
        public string Title { get; set; } = string.Empty;
        public string PersianDesc { get; set; } = string.Empty;
        public int Score { get; set; }
        public double Percentage { get; set; }
        public string ColorHex { get; set; } = "#1976d2";
        public string Icon { get; set; } = string.Empty;
    }

    public class HollandResult
    {
        public string HollandCode { get; set; } = string.Empty; // مثلاً IAS یا SEC
        public List<HollandScore> Scores { get; set; } = new();
        public List<string> HighSchoolBranches { get; set; } = new(); // هدایت تحصیلی نهم
        public List<string> UniversityMajors { get; set; } = new();   // انتخاب رشته کنکور
        public List<string> SuitableCareers { get; set; } = new();   // مشاغل پیشنهادی
        public string PersonalityAnalysis { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; } = DateTime.Now;
    }
}
