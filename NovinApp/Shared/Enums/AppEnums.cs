namespace NovinApp.Shared.Enums
{
    public enum StudyTaskStatus 
    { 
        Planned,       // در انتظار انجام
        InProgress,    // در حال انجام
        Completed,     // انجام کامل
        Incomplete,    // ناقص
        Skipped,       // انجام نشد
        Cancelled      // لغو شده
    }

    public enum ImportStatus 
    { 
        Pending, 
        Processing, 
        Completed, 
        Failed 
    }

    public static class ActivityTypes
    {
        public const string Test = "تست";
        public const string Study = "مطالعه کتاب یا جزوه یا درس نامه";
        public const string Video = "فیلم آموزشی";
        public const string Class = "کلاس";
        public const string DescriptiveQuestions = "سوالات تشریحی";
        public const string InPersonCounseling = "مشاوره حضوری";
        public const string OnlineCounseling = "مشاوره آنلاین";
        public const string Exam = "آزمون آزمایشی";
        public const string Review = "مرور و جمع‌بندی";
        public const string Other = "سایر فعالیت‌ها";

        public static readonly string[] All = new[]
        {
            Test, Study, Video, Class, DescriptiveQuestions, InPersonCounseling, OnlineCounseling, Exam, Review, Other
        };
    }

    public static class TestTypes
    {
        public const string Educational = "آموزشی";
        public const string Mastery = "تسلط";
        public const string Review = "مرور";
        public const string Marked = "مارک دار";
        public const string Timed = "زمان‌دار";
        public const string Comprehensive = "جامع";

        public static readonly string[] All = new[]
        {
            Educational, Mastery, Review, Marked, Timed, Comprehensive
        };
    }

    public static class TestSources
    {
        public const string Jozveh = "جزوه";
        public const string Dinamik = "دینامیک";
        public const string GajSilver = "گاج نقره ای";
        public const string GajGold = "گاج طلایی";
        public const string KheiliSabz = "خیلی سبز";
        public const string Mobtakeran = "مبتکران";
        public const string IQ = "IQ";
        public const string Nardeban = "نردبان";
        public const string Ghalamchi = "قلم چی";
        public const string SeSathi = "سه سطحی";
        public const string Formool20 = "فرمول 20";
        public const string MehroMah = "مهر و ماه";
        public const string Moshaveran = "مشاوران";
        public const string MowjAzmoon = "موج آزمون";
        public const string FaslAzmoon = "فصل آزمون";
        public const string Pinocchio = "پینوکیو";
        public const string Other = "سایر";

        public static readonly string[] All = new[]
        {
            Jozveh, Dinamik, GajSilver, GajGold, KheiliSabz, Mobtakeran, IQ, Nardeban,
            Ghalamchi, SeSathi, Formool20, MehroMah, Moshaveran, MowjAzmoon, FaslAzmoon, Pinocchio, Other
        };
    }

    public static class QualityRatings
    {
        public const int Excellent = 5;
        public const int Good = 4;
        public const int Medium = 3;
        public const int Weak = 2;
        public const int VeryWeak = 1;

        public static string GetLabel(int rating) => rating switch
        {
            5 => "عالی 😍",
            4 => "خوب 😊",
            3 => "متوسط 😐",
            2 => "ضعیف 😞",
            1 => "خیلی ضعیف 😡",
            _ => "نامشخص"
        };
    }

    public static class GradeLevels
    {
        public const string Grade7 = "هفتم";
        public const string Grade8 = "هشتم";
        public const string Grade9 = "نهم";
        public const string Grade10 = "دهم";
        public const string Grade11 = "یازدهم";
        public const string Grade12 = "دوازدهم";
        public const string Graduate = "فارغ‌التحصیل / کنکور";
        public const string Technical = "فنی و حرفه‌ای";

        public static readonly string[] All = new[]
        {
            Grade7, Grade8, Grade9, Grade10, Grade11, Grade12, Graduate, Technical
        };
    }

    public static class FieldsOfStudy
    {
        public const string Experimental = "تجربی";
        public const string Mathematics = "ریاضی";
        public const string Humanities = "انسانی";
        public const string Art = "هنر";
        public const string Language = "زبان";
        public const string Computer = "کامپیوتر";
        public const string Accounting = "حسابداری";
        public const string General = "عمومی / متوسطه اول";

        public static readonly string[] All = new[]
        {
            Experimental, Mathematics, Humanities, Art, Language, Computer, Accounting, General
        };
    }

    public enum CounselingStatus
    {
        Pending,
        Approved,
        Rejected,
        Completed,
        Cancelled
    }
}
