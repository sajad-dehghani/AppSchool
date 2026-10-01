using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NovinApp.Server.Models;
using NovinApp.Shared;
using NovinApp.Shared.Entities;

namespace NovinApp.Server.MyContext
{
    public class MyAppContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public MyAppContext()
        {
        }

        public MyAppContext(DbContextOptions<MyAppContext> options) : base(options)
        {
        }

        // Domain Entities
        public DbSet<School> Schools => Set<School>();
        public DbSet<ConsultantProfile> ConsultantProfiles => Set<ConsultantProfile>();
        public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
        public DbSet<Exam> ExamsList => Set<Exam>();
        public DbSet<Subject> Subjects => Set<Subject>();
        public DbSet<StudentExamResult> StudentExamResults => Set<StudentExamResult>();
        public DbSet<StudentExamSubjectResult> StudentExamSubjectResults => Set<StudentExamSubjectResult>();
        public DbSet<StudyPlan> StudyPlans => Set<StudyPlan>();
        public DbSet<StudyTask> StudyTasks => Set<StudyTask>();
        public DbSet<NovinApp.Shared.Entities.StudyDailyReport> StudyDailyReports => Set<NovinApp.Shared.Entities.StudyDailyReport>();
        public DbSet<ImportSession> ImportSessions => Set<ImportSession>();
        public DbSet<ImportError> ImportErrors => Set<ImportError>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        // Psychological Tests
        public DbSet<PsychologyTestResult> PsychologyTestResults => Set<PsychologyTestResult>();

        // Legacy DbSets for Backward Compatibility
        public DbSet<Report_students> Report_students => Set<Report_students>();
        public DbSet<Question> Question => Set<Question>();
        public DbSet<Exams> Exams => Set<Exams>();
        public DbSet<User> User => Set<User>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // National Code uniqueness
            builder.Entity<ApplicationUser>(b =>
            {
                b.HasIndex(u => u.NationalCode)
                    .IsUnique()
                    .HasFilter("[NationalCode] IS NOT NULL");
            });

            // StudentProfile configuration
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

            // StudentExamResult configuration
            builder.Entity<StudentExamResult>(b =>
            {
                b.HasIndex(r => new { r.StudentProfileId, r.ExamId });

                b.HasOne(r => r.StudentProfile)
                    .WithMany(s => s.ExamResults)
                    .HasForeignKey(r => r.StudentProfileId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(r => r.Exam)
                    .WithMany(e => e.Results)
                    .HasForeignKey(r => r.ExamId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // StudentExamSubjectResult configuration
            builder.Entity<StudentExamSubjectResult>(b =>
            {
                b.HasOne(sr => sr.StudentExamResult)
                    .WithMany(r => r.SubjectResults)
                    .HasForeignKey(sr => sr.StudentExamResultId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(sr => sr.Subject)
                    .WithMany()
                    .HasForeignKey(sr => sr.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // StudyPlan configuration
            builder.Entity<StudyPlan>(b =>
            {
                b.HasOne(p => p.StudentProfile)
                    .WithMany(s => s.StudyPlans)
                    .HasForeignKey(p => p.StudentProfileId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(p => p.ConsultantProfile)
                    .WithMany(c => c.StudyPlans)
                    .HasForeignKey(p => p.ConsultantProfileId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // StudyDailyReport configuration
            builder.Entity<NovinApp.Shared.Entities.StudyDailyReport>(b =>
            {
                b.HasOne(r => r.StudentProfile)
                    .WithMany(s => s.DailyReports)
                    .HasForeignKey(r => r.StudentProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(r => r.StudyPlan)
                    .WithMany(p => p.DailyReports)
                    .HasForeignKey(r => r.StudyPlanId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ImportSession & ImportError
            builder.Entity<ImportError>(b =>
            {
                b.HasOne(e => e.ImportSession)
                    .WithMany(s => s.Errors)
                    .HasForeignKey(e => e.ImportSessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RefreshToken
            builder.Entity<RefreshToken>(b =>
            {
                b.HasIndex(r => r.Token);
                b.HasIndex(r => r.UserId);
            });
        }
    }
}
