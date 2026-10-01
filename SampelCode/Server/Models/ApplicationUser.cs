using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using NovinApp.Shared.Entities;

namespace NovinApp.Server.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string NationalCode { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(500)]
        public string? ProfilePictureUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }

        public virtual StudentProfile? StudentProfile { get; set; }
        public virtual ConsultantProfile? ConsultantProfile { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }

    public class ApplicationRole : IdentityRole<int>
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName, string persianTitle) : base(roleName)
        {
            PersianTitle = persianTitle;
        }

        [MaxLength(100)]
        public string PersianTitle { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }
    }
}
