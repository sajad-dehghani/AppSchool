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
        [Required][MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        public long? SchoolCode { get; set; }
        [MaxLength(100)] public string? RegionName { get; set; }
        [MaxLength(200)] public string? Address { get; set; }
        [MaxLength(30)] public string? PhoneNumber { get; set; }
        public int? ManagerUserId { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual ICollection<StudentProfile> Students { get; set; } = new List<StudentProfile>();
    }

    [Table("ConsultantProfiles")]
    public class ConsultantProfile : BaseEntity
    {
        [Required] public int UserId { get; set; }
        [MaxLength(150)] public string? Specialty { get; set; }
        [MaxLength(500)] public string? Bio { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual ICollection<StudentProfile> Students { get; set; } = new List<StudentProfile>();
    }

    [Table("StudentProfiles")]
    public class StudentProfile : BaseEntity
    {
        [Required] public int UserId { get; set; }
        [Required][MaxLength(20)] public string NationalCode { get; set; } = string.Empty;
        public long? StudentCode { get; set; }
        [MaxLength(100)] public string? FatherName { get; set; }
        [MaxLength(100)] public string? GradeLevel { get; set; }
        [MaxLength(100)] public string? FieldOfStudy { get; set; }
        public int? SchoolId { get; set; }
        public virtual School? School { get; set; }
        public int? ConsultantId { get; set; }
        public virtual ConsultantProfile? Consultant { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
