namespace NovinApp.Shared.Constants
{
    public static class UserRoles
    {
        public const string SystemAdmin = "SystemAdmin";
        public const string SchoolManager = "SchoolManager";
        public const string Consultant = "Consultant";
        public const string Student = "Student";

        // عناوین فارسی نقش‌ها جهت نمایش در رابط کاربری
        public const string SystemAdminPersian = "مدیر سیستم";
        public const string SchoolManagerPersian = "مدیر مدرسه";
        public const string ConsultantPersian = "مشاور تحصیلی";
        public const string StudentPersian = "دانش‌آموز";

        public static readonly string[] AllRoles = new[]
        {
            SystemAdmin,
            SchoolManager,
            Consultant,
            Student
        };

        public static string GetPersianTitle(string role)
        {
            return role switch
            {
                SystemAdmin => SystemAdminPersian,
                SchoolManager => SchoolManagerPersian,
                Consultant => ConsultantPersian,
                Student => StudentPersian,
                // پشتیبانی از مقادیر احتمالی قدیمی
                "مدیر" or "مدیران_مدارس" => SchoolManagerPersian,
                "مشاور" => ConsultantPersian,
                "دانش آموز" or "دانش_آموز" => StudentPersian,
                _ => role
            };
        }

        public static string NormalizeRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return Student;
            var trimmed = role.Trim();

            if (trimmed.Equals(SystemAdmin, System.StringComparison.OrdinalIgnoreCase) || trimmed.Contains("ادمین") || trimmed.Contains("مدیر سیستم"))
                return SystemAdmin;

            if (trimmed.Equals(SchoolManager, System.StringComparison.OrdinalIgnoreCase) || trimmed.Contains("مدیر مدرسه") || trimmed.Contains("مدیران_مدارس"))
                return SchoolManager;

            if (trimmed.Equals(Consultant, System.StringComparison.OrdinalIgnoreCase) || trimmed.Contains("مشاور"))
                return Consultant;

            if (trimmed.Equals(Student, System.StringComparison.OrdinalIgnoreCase) || trimmed.Contains("دانش آموز") || trimmed.Contains("دانش_آموز"))
                return Student;

            return Student;
        }
    }
}
