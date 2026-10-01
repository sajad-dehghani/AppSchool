namespace NovinApp.Shared.Constants
{
    public static class UserRoles
    {
        public const string SystemAdmin = "SystemAdmin";
        public const string SchoolManager = "SchoolManager";
        public const string Consultant = "Consultant";
        public const string Student = "Student";

        public const string SystemAdminPersian = "مدیر سیستم";
        public const string SchoolManagerPersian = "مدیر مدرسه";
        public const string ConsultantPersian = "مشاور تحصیلی";
        public const string StudentPersian = "دانش‌آموز";

        public static readonly string[] AllRoles = new[]
        {
            SystemAdmin, SchoolManager, Consultant, Student
        };

        public static string GetPersianTitle(string role)
        {
            var normalized = NormalizeRole(role);
            return normalized switch
            {
                SystemAdmin => SystemAdminPersian,
                SchoolManager => SchoolManagerPersian,
                Consultant => ConsultantPersian,
                Student => StudentPersian,
                _ => role
            };
        }

        public static string NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role)) return Student;
            var t = role.Trim();

            // مدیر مدرسه
            if (t.Equals(SchoolManager, StringComparison.OrdinalIgnoreCase) ||
                t.Contains("مدیر مدرسه") ||
                t.Contains("مدیران_مدارس") ||
                t.Contains("مدیران مدارس"))
                return SchoolManager;

            // مدیر سیستم / ادمین
            if (t.Equals(SystemAdmin, StringComparison.OrdinalIgnoreCase) ||
                t.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("Administrator", StringComparison.OrdinalIgnoreCase) ||
                t.Contains("ادمین") ||
                t.Contains("مدیر سیستم") ||
                t.Contains("مدیر ارشد") ||
                t.Contains("مدیر کل") ||
                t.Contains("مدیرکل") ||
                t.Equals("مدیر"))
                return SystemAdmin;

            // مشاور
            if (t.Equals(Consultant, StringComparison.OrdinalIgnoreCase) ||
                t.Equals("Moshaver", StringComparison.OrdinalIgnoreCase) ||
                t.Contains("مشاور"))
                return Consultant;

            // دانش آموز
            if (t.Equals(Student, StringComparison.OrdinalIgnoreCase) ||
                t.Equals("DaneshAmoz", StringComparison.OrdinalIgnoreCase) ||
                t.Contains("دانش"))
                return Student;

            return Student;
        }
    }
}
