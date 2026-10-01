using System.ComponentModel.DataAnnotations;
using NovinApp.Shared.Enums;

namespace NovinApp.Shared.DTOs.Import
{
    /// <summary>
    /// نتیجه ایمپورت اکسل
    /// </summary>
    public class ImportResultDto
    {
        public int ImportSessionId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public ImportStatus Status { get; set; }
        public int TotalRows { get; set; }
        public int SuccessfulRows { get; set; }
        public int FailedRows { get; set; }
        public int WarningRows { get; set; }
        public string? SummaryMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ImportErrorDto> Errors { get; set; } = new();
    }

    /// <summary>
    /// جزئیات هر خطای ردیف ایمپورت
    /// </summary>
    public class ImportErrorDto
    {
        public int RowNumber { get; set; }
        public string? ColumnName { get; set; }
        public string? RawValue { get; set; }
        public string ErrorType { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// تنظیمات آپلود فایل اکسل (از کلاینت ارسال می‌شود)
    /// </summary>
    public class ImportSettingsDto
    {
        /// <summary>
        /// آیدی آزمون مرتبط (اگر نتایج آزمون ایمپورت می‌شود)
        /// </summary>
        public int? ExamId { get; set; }

        /// <summary>
        /// نحوه برخورد با رکوردهای تکراری (بر اساس کدملی)
        /// </summary>
        public DuplicateHandlingMode DuplicateHandling { get; set; } = DuplicateHandlingMode.Skip;
    }

    /// <summary>
    /// DTO لیست تاریخچه ایمپورت‌ها
    /// </summary>
    public class ImportSessionListDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public ImportStatus Status { get; set; }
        public int TotalRows { get; set; }
        public int SuccessfulRows { get; set; }
        public int FailedRows { get; set; }
        public string? SummaryMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ExamTitle { get; set; }
    }
}
