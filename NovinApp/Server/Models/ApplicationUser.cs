using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NovinApp.Server.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        [MaxLength(150)]
        public string? FullName { get; set; }

        [MaxLength(20)]
        public string? NationalCode { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public class ApplicationRole : IdentityRole<int>
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }
    }
}
