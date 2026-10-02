using System;
using System.Collections.Generic;
using System.Globalization;

namespace NovinApp.Shared.Helpers
{
    public static class PersianDateHelper
    {
        private static readonly PersianCalendar pc = new();

        public static string ToPersianDateString(DateTime date)
        {
            int year = pc.GetYear(date);
            int month = pc.GetMonth(date);
            int day = pc.GetDayOfMonth(date);
            return $"{year:0000}/{month:02}/{day:02}";
        }

        public static string NowPersianDate()
        {
            return ToPersianDateString(DateTime.Now);
        }

        public static string GetPersianDayOfWeek(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Saturday => "شنبه",
                DayOfWeek.Sunday => "یکشنبه",
                DayOfWeek.Monday => "دوشنبه",
                DayOfWeek.Tuesday => "سه‌شنبه",
                DayOfWeek.Wednesday => "چهارشنبه",
                DayOfWeek.Thursday => "پنج‌شنبه",
                DayOfWeek.Friday => "جمعه",
                _ => ""
            };
        }

        public static string GetPersianMonthName(int month)
        {
            return month switch
            {
                1 => "فروردین",
                2 => "اردیبهشت",
                3 => "خرداد",
                4 => "تیر",
                5 => "مرداد",
                6 => "شهریور",
                7 => "مهر",
                8 => "آبان",
                9 => "آذر",
                10 => "دی",
                11 => "بهمن",
                12 => "اسفند",
                _ => ""
            };
        }

        public static bool TryParsePersianDate(string persianDateStr, out DateTime result)
        {
            result = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(persianDateStr)) return false;

            try
            {
                var parts = persianDateStr.Replace("-", "/").Trim().Split('/');
                if (parts.Length != 3) return false;

                int y = int.Parse(parts[0]);
                int m = int.Parse(parts[1]);
                int d = int.Parse(parts[2]);

                result = pc.ToDateTime(y, m, d, 0, 0, 0, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static DateTime PersianToGregorian(int year, int month, int day)
        {
            return pc.ToDateTime(year, month, day, 0, 0, 0, 0);
        }

        public static (int Year, int Month, int Day) GetPersianParts(DateTime date)
        {
            return (pc.GetYear(date), pc.GetMonth(date), pc.GetDayOfMonth(date));
        }

        public static int GetDaysInPersianMonth(int year, int month)
        {
            return pc.GetDaysInMonth(year, month);
        }

        public static CultureInfo GetPersianCulture()
        {
            var culture = new CultureInfo("fa-IR");
            DateTimeFormatInfo formatInfo = culture.DateTimeFormat;
            formatInfo.AbbreviatedDayNames = new[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };
            formatInfo.DayNames = new[] { "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه", "شنبه" };
            var monthNames = new[]
            {
                "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", ""
            };
            formatInfo.AbbreviatedMonthNames = formatInfo.MonthNames = formatInfo.MonthGenitiveNames = monthNames;
            formatInfo.AMDesignator = "ق.ظ";
            formatInfo.PMDesignator = "ب.ظ";
            formatInfo.ShortDatePattern = "yyyy/MM/dd";
            formatInfo.LongDatePattern = "dddd, dd MMMM, yyyy";
            formatInfo.FirstDayOfWeek = DayOfWeek.Saturday;
            return culture;
        }

        /// <summary>
        /// بازگرداندن روزهای یک هفته شمسی (از شنبه تا جمعه)
        /// </summary>
        public static List<PersianCalendarDayInfo> GetWeekDays(DateTime targetDate)
        {
            var days = new List<PersianCalendarDayInfo>();
            // شنبه را پیدا کن
            int offset = (int)targetDate.DayOfWeek;
            int daysSinceSaturday = (offset + 1) % 7;
            var saturday = targetDate.AddDays(-daysSinceSaturday);

            for (int i = 0; i < 7; i++)
            {
                var dt = saturday.AddDays(i);
                var (y, m, d) = GetPersianParts(dt);
                days.Add(new PersianCalendarDayInfo
                {
                    GregorianDate = dt.Date,
                    PersianDate = ToPersianDateString(dt),
                    Year = y,
                    Month = m,
                    Day = d,
                    DayOfWeekName = GetPersianDayOfWeek(dt),
                    IsToday = dt.Date == DateTime.Now.Date,
                    IsCurrentMonth = true
                });
            }

            return days;
        }

        /// <summary>
        /// ساخت ماتریس روزهای ماه برای تقویم ماهانه
        /// </summary>
        public static List<PersianCalendarDayInfo> GetMonthCalendarDays(int persianYear, int persianMonth)
        {
            var result = new List<PersianCalendarDayInfo>();
            var firstDayGregorian = pc.ToDateTime(persianYear, persianMonth, 1, 0, 0, 0, 0);
            int daysInMonth = pc.GetDaysInMonth(persianYear, persianMonth);

            // محاسبه تعداد روزهای خالی قبل از روز اول (شنبه = 0، یکشنبه = 1، ... جمعه = 6)
            int firstDayOfWeekOffset = ((int)firstDayGregorian.DayOfWeek + 1) % 7;

            // روزهای ماه قبل جهت پر کردن ردیف اول تقویم
            for (int i = firstDayOfWeekOffset; i > 0; i--)
            {
                var prevDt = firstDayGregorian.AddDays(-i);
                var (py, pm, pd) = GetPersianParts(prevDt);
                result.Add(new PersianCalendarDayInfo
                {
                    GregorianDate = prevDt.Date,
                    PersianDate = ToPersianDateString(prevDt),
                    Year = py,
                    Month = pm,
                    Day = pd,
                    DayOfWeekName = GetPersianDayOfWeek(prevDt),
                    IsToday = prevDt.Date == DateTime.Now.Date,
                    IsCurrentMonth = false
                });
            }

            // روزهای ماه جاری
            for (int d = 1; d <= daysInMonth; d++)
            {
                var curDt = pc.ToDateTime(persianYear, persianMonth, d, 0, 0, 0, 0);
                result.Add(new PersianCalendarDayInfo
                {
                    GregorianDate = curDt.Date,
                    PersianDate = ToPersianDateString(curDt),
                    Year = persianYear,
                    Month = persianMonth,
                    Day = d,
                    DayOfWeekName = GetPersianDayOfWeek(curDt),
                    IsToday = curDt.Date == DateTime.Now.Date,
                    IsCurrentMonth = true
                });
            }

            // روزهای ماه بعد جهت تکمیل ۶ ردیف تقویم (۴۲ خانه)
            int totalSoFar = result.Count;
            int remaining = 42 - totalSoFar;
            if (remaining > 7) remaining -= 7; // اگر ۳۵ خانه کافی بود

            var lastDayOfMonth = pc.ToDateTime(persianYear, persianMonth, daysInMonth, 0, 0, 0, 0);
            for (int i = 1; i <= remaining; i++)
            {
                var nextDt = lastDayOfMonth.AddDays(i);
                var (py, pm, pd) = GetPersianParts(nextDt);
                result.Add(new PersianCalendarDayInfo
                {
                    GregorianDate = nextDt.Date,
                    PersianDate = ToPersianDateString(nextDt),
                    Year = py,
                    Month = pm,
                    Day = pd,
                    DayOfWeekName = GetPersianDayOfWeek(nextDt),
                    IsToday = nextDt.Date == DateTime.Now.Date,
                    IsCurrentMonth = false
                });
            }

            return result;
        }
    }

    public class PersianCalendarDayInfo
    {
        public DateTime GregorianDate { get; set; }
        public string PersianDate { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public string DayOfWeekName { get; set; } = string.Empty;
        public bool IsToday { get; set; }
        public bool IsCurrentMonth { get; set; }
    }
}
