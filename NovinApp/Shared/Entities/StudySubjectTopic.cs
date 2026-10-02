using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovinApp.Shared.Entities
{
    [Table("StudySubjectTopics")]
    public class StudySubjectTopic : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string GradeLevel { get; set; } = string.Empty; // دهم، یازدهم، دوازدهم...

        [Required]
        [MaxLength(50)]
        public string FieldOfStudy { get; set; } = string.Empty; // تجربی، ریاضی، انسانی...

        [Required]
        [MaxLength(100)]
        public string Subject { get; set; } = string.Empty; // زیست‌شناسی، شیمی...

        [Required]
        [MaxLength(150)]
        public string Chapter { get; set; } = string.Empty; // فصل ۱: زیست‌شناسی، دیروز، امروز و فردا

        [Required]
        [MaxLength(250)]
        public string Topic { get; set; } = string.Empty; // یاخته و بافت‌های جانوری

        [MaxLength(500)]
        public string? Description { get; set; }

        public int PriorityOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public int? CreatedByUserId { get; set; }
    }
}
