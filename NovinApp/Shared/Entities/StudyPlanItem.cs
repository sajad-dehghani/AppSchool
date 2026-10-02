using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NovinApp.Shared.Enums;

namespace NovinApp.Shared.Entities
{
    [Table("StudyPlanItems")]
    public class StudyPlanItem : BaseEntity
    {
        // انتساب به مشاور و دانش‌آموز
        [Required]
        public int ConsultantUserId { get; set; }
        
        [MaxLength(150)]
        public string? ConsultantName { get; set; }

        [Required]
        public int StudentUserId { get; set; }

        [MaxLength(150)]
        public string? StudentName { get; set; }

        public int? SchoolId { get; set; }

        // زمان‌بندی و تاریخ
        [Required]
        [MaxLength(20)]
        public string PersianDate { get; set; } = string.Empty; // مثلا 1403/07/15

        public DateTime GregorianDate { get; set; } = DateTime.Now;

        [MaxLength(10)]
        public string StartTime { get; set; } = "08:00"; // مثلا 08:30

        public int DurationMinutes { get; set; } = 60; // مدت زمان به دقیقه

        // مشخصات برنامه آموزشی
        [Required]
        [MaxLength(100)]
        public string ActivityType { get; set; } = ActivityTypes.Study; // تست، مطالعه، فیلم، کلاس، سوالات تشریحی، مشاوره...

        public int? TestCount { get; set; } // تعداد تست تعیین شده

        [MaxLength(50)]
        public string? TestType { get; set; } // آموزشی، تسلط، مرور، مارک‌دار

        [MaxLength(50)]
        public string? GradeLevel { get; set; } // دهم، یازدهم، دوازدهم...

        [MaxLength(50)]
        public string? FieldOfStudy { get; set; } // تجربی، ریاضی، انسانی...

        [MaxLength(100)]
        public string? Subject { get; set; } // زیست‌شناسی، شیمی، فیزیک...

        [MaxLength(150)]
        public string? Chapter { get; set; } // فصل اول

        [MaxLength(250)]
        public string? Topic { get; set; } // مبحث

        [MaxLength(1000)]
        public string? ConsultantNote { get; set; } // توضیحات مشاور

        [MaxLength(500)]
        public string? ConsultantAttachmentUrl { get; set; } // فایل پیوست مشاور

        [MaxLength(200)]
        public string? ConsultantAttachmentName { get; set; }

        // جلسات مشاوره حضوری یا آنلاین
        public bool IsCounselingSession { get; set; } = false;

        [MaxLength(50)]
        public string? CounselingType { get; set; } // حضوری / آنلاین

        [MaxLength(500)]
        public string? MeetingLink { get; set; } // لینک جلسه آنلاین (Skyroom/Google Meet/Adobe Connect)

        // وضعیت کلی
        public StudyTaskStatus Status { get; set; } = StudyTaskStatus.Planned;

        // === فیلدهای بازخورد و گزارش کار دانش‌آموز ===
        public bool IsFeedbackSubmitted { get; set; } = false;

        public DateTime? FeedbackSubmittedAt { get; set; }

        [MaxLength(50)]
        public string? StudentStatus { get; set; } // انجام شد، ناقص، انجام نشد

        public int? QualityRating { get; set; } // 1 تا 5

        [MaxLength(100)]
        public string? QualityText { get; set; } // توصیفی

        public int? StudyPercentage { get; set; } // درصد مطالعه (0 تا 100)

        public int? CorrectTests { get; set; } // تست‌های صحیح

        public int? WrongTests { get; set; } // تست‌های غلط

        public int? UnansweredTests { get; set; } // تست‌های نزده

        public int? ActualDurationMinutes { get; set; } // مدت زمان مطالعه واقعی انجام شده

        [MaxLength(1000)]
        public string? StudentNote { get; set; } // توضیحات دانش‌آموز

        [MaxLength(500)]
        public string? StudentAttachmentUrl { get; set; } // فایل پیوست دانش‌آموز

        [MaxLength(200)]
        public string? StudentAttachmentName { get; set; }

        [MaxLength(100)]
        public string? TestSource { get; set; } // منبع تست (خیلی سبز، گاج، مبتکران...)
    }
}
