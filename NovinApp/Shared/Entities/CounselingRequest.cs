using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NovinApp.Shared.Enums;

namespace NovinApp.Shared.Entities
{
    [Table("CounselingRequests")]
    public class CounselingRequest : BaseEntity
    {
        [Required]
        public int StudentUserId { get; set; }

        [MaxLength(150)]
        public string? StudentName { get; set; }

        public int? ConsultantUserId { get; set; }

        [MaxLength(150)]
        public string? ConsultantName { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequestType { get; set; } = "مشاوره آنلاین"; // مشاوره آنلاین / مشاوره حضوری

        [MaxLength(20)]
        public string? PreferredDate { get; set; } // تاریخ پیشنهادی شمسی مثلا 1403/07/20

        [MaxLength(10)]
        public string? PreferredTime { get; set; } // ساعت پیشنهادی

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty; // توضیحات دانش‌آموز

        [MaxLength(500)]
        public string? MeetingLink { get; set; } // لینک جلسه در صورت نیاز

        [MaxLength(1000)]
        public string? ConsultantResponse { get; set; } // پاسخ یا توضیحات مشاور

        [MaxLength(20)]
        public string? ScheduledDate { get; set; } // تاریخ قطعی تنظیم شده

        [MaxLength(10)]
        public string? ScheduledTime { get; set; } // ساعت قطعی تنظیم شده

        public CounselingStatus Status { get; set; } = CounselingStatus.Pending;

        public int? CreatedStudyPlanId { get; set; } // شناسه برنامه‌ای که به آن تبدیل شد
    }
}
