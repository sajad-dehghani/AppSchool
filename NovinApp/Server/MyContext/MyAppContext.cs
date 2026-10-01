using Microsoft.EntityFrameworkCore;
using NovinApp.Server.Models;
using NovinApp.Shared;
using NovinApp.Shared.Entities;

namespace NovinApp.Server.MyContext
{
    public class MyAppContext : DbContext
    {
        public MyAppContext() { }

        public MyAppContext(DbContextOptions<MyAppContext> options) : base(options) { }

        // ===  جداول اصلی کاربران (موجود) ===
        public DbSet<User> User { get; set; }
        public DbSet<Report_students> Report_students { get; set; }

        // === جداول جدید سیستم مدیریت ===
        public DbSet<School> Schools => Set<School>();
        public DbSet<ConsultantProfile> ConsultantProfiles => Set<ConsultantProfile>();
        public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<StudentProfile>(b =>
            {
                b.HasIndex(s => s.NationalCode);
                b.HasOne(s => s.School)
                    .WithMany(sc => sc.Students)
                    .HasForeignKey(s => s.SchoolId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(s => s.Consultant)
                    .WithMany(c => c.Students)
                    .HasForeignKey(s => s.ConsultantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<User>(b =>
            {
                b.HasIndex(u => u.code_meli);
                b.HasIndex(u => u.Rool);
                b.HasIndex(u => u.Id_School);
                b.HasIndex(u => u.Id_Moshaver);
                b.HasIndex(u => u.active);
            });

            builder.Entity<School>()
                .HasIndex(s => s.Name);
        }
    }
}
