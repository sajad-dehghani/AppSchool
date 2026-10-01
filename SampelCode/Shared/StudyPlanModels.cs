using System;
using System.Collections.Generic;

namespace NovinApp.Shared
{
    public class StudyDailyReport
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; } = DateTime.Today;
        public string SubjectName { get; set; } = string.Empty; // زیست‌شناسی، ریاضی، فیزیک، شیمی، ...
        public double StudyHours { get; set; } // ساعت مطالعه
        public int TestCount { get; set; } // تعداد تست حل‌شده
        public int CorrectCount { get; set; } // تست درست
        public int WrongCount { get; set; } // تست غلط
        public string TopicsCovered { get; set; } = string.Empty; // مباحث مطالعه‌شده
        public string StudentNote { get; set; } = string.Empty; // یادداشت دانش‌آموز
        public string CounselorNote { get; set; } = string.Empty; // نظر و بازخورد مشاور
        public int QualityRating { get; set; } = 4; // میزان رضایت و کیفیت مطالعه ۱ تا ۵
    }

    public class WeeklyPlanSchedule
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CounselorId { get; set; }
        public string WeekTitle { get; set; } = string.Empty; // برنامه هفته اول مهر
        public string TargetBranch { get; set; } = "تجربی"; // تجربی، ریاضی، انسانی
        public double TargetWeeklyHours { get; set; } = 45; // هدف ساعات هفتگی
        public int TargetWeeklyTests { get; set; } = 800; // هدف تست هفتگی
        public string Description { get; set; } = string.Empty; // توصیه‌های مشاور برای کنکور
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
