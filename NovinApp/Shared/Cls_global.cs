using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


namespace NovinApp.Shared
{


    public class Cls_global
    {
        public enum Enum_Rolse
        { 
           دانش_آموز,
           مدیران_مدارس,
           مدیران_منطقه,
           مدیران_اداره,
           پرسنل
        }


        public static bool IsEdit = false;

        public static string URL = "";
        public static int RolesId = 0;
        public static string select_sheet = "1";
        public static string Rolse = "";
        public static string id = "";
        public static string refinery_name = "";
        public static string semat_name = "";
        public static string code_meli = "";
        public static string code_p = "";
        public static string fname = "";
        public static string gender = "";
        public static string pic = "";
        public static string Student_School_Name = "";
        public static string Student_Region_Name = "";
        public static string Student_School_Code = "";
        public static string Student_LevelNameString = "";
        public static string Student_Section_Name = "";
        public static string StringName = "";
        public static int ExamStatictics_PresentCount = 0;
        public static int SchoolStatistics_PresentCount = 0;
        public static int GenderStatistics_PresentCount = 0;
        public static int RegionStatistics_PresentCount = 0;

        //public static HashSet<User> selectedUser = new HashSet<User>();
        //public static User Edit_personel = new User();
        //public static User personel_one = new User();

        public static string sel_tar1 = "";
        public static int sel_personel = 0;
        public static string sel_tar2 = "";
        public static bool mode = false;

        public static int select_row_id = 0;
        public static string select_row_titr = "";
        public static string get_month(string tar)
        {
            if (tar != "")
            {
                string mah;
                if (tar.Length == 2)
                {
                    mah = tar;
                }
                else
                {
                    mah = tar.Substring(5, 2);
                }
                if (mah == "01") { return "فروردین"; }
                else if (mah == "02") { return "اردیبهشت"; }
                else if (mah == "03") { return "خرداد"; }
                else if (mah == "04") { return "تیر"; }
                else if (mah == "05") { return "مرداد"; }
                else if (mah == "06") { return "شهریور"; }
                else if (mah == "07") { return "مهر"; }
                else if (mah == "08") { return "آبان"; }
                else if (mah == "09") { return "آذر"; }
                else if (mah == "10") { return "دی"; }
                else if (mah == "11") { return "بهمن"; }
                else if (mah == "12") { return "اسفند"; }
                else
                    return "Error";
            }
            else
                return "";

        }
        public static string get_month_num(string mah)
        {
            if (mah != "")
            {

                if (mah == "فروردین") { return "01"; }
                else if (mah == "اردیبهشت") { return "02"; }
                else if (mah == "خرداد") { return "03"; }
                else if (mah == "تیر") { return "04"; }
                else if (mah == "مرداد") { return "05"; }
                else if (mah == "شهریور") { return "06"; }
                else if (mah == "مهر") { return "07"; }
                else if (mah == "آبان") { return "08"; }
                else if (mah == "آذر") { return "09"; }
                else if (mah == "دی") { return "10"; }
                else if (mah == "بهمن") { return "11"; }
                else if (mah == "اسفند") { return "12"; }
                else
                    return "00";
            }
            else
                return "00";

        }
        public static string get_year(string tar)
        {
            if (tar != "")
            {
                return tar.Substring(0, 4);
            }
            else
                return "";

        }
        public static string tar_fa(bool istext)
        {
            PersianCalendar pc = new PersianCalendar();
            string hafte = new PersianCalendar().GetDayOfWeek(DateTime.Now).ToString();
            if (hafte == "Sunday")
            {
                hafte = "یکشنبه";
            }
            else if (hafte == "Monday")
            {
                hafte = "دوشنبه";
            }
            else if (hafte == "Tuesday")
            {
                hafte = "سه شنبه";
            }
            else if (hafte == "Wednesday")
            {
                hafte = "چهار شنبه";
            }
            else if (hafte == "Thursday")
            {
                hafte = "پنجشنبه";
            }
            else if (hafte == "Friday")
            {
                hafte = "جمعه";
            }
            else if (hafte == "Saturday")
            {
                hafte = "شنبه";
            }
            string mah = pc.GetMonth(DateTime.Now).ToString();
            if (mah == "1") { mah = "فروردین"; }
            else if (mah == "2") { mah = "اردیبهشت"; }
            else if (mah == "3") { mah = "خرداد"; }
            else if (mah == "4") { mah = "تیر"; }
            else if (mah == "5") { mah = "مرداد"; }
            else if (mah == "6") { mah = "شهریور"; }
            else if (mah == "7") { mah = "مهر"; }
            else if (mah == "8") { mah = "آبان"; }
            else if (mah == "9") { mah = "آذر"; }
            else if (mah == "10") { mah = "دی"; }
            else if (mah == "11") { mah = "بهمن"; }
            else if (mah == "12") { mah = "اسفند"; }
            if (istext)
            {
                return "امروز " + hafte + " " + new PersianCalendar().GetDayOfMonth(DateTime.Now).ToString() + " " + mah + " ";
            }
            else
            {
                return pc.GetYear(DateTime.Now) + "/" + (pc.GetMonth(DateTime.Now).ToString().Count() == 1 ? "0" + pc.GetMonth(DateTime.Now) : pc.GetMonth(DateTime.Now)) + "/" + (pc.GetDayOfMonth(DateTime.Now).ToString().Count() == 1 ? "0" + pc.GetDayOfMonth(DateTime.Now) : pc.GetDayOfMonth(DateTime.Now));
            }

        }
        public static string tar_fa(int yare, int month, int day)
        {
            PersianCalendar pc = new PersianCalendar();
            DateTime date = DateTime.Now;
            date = date.AddDays(day);
            date = date.AddMonths(month);
            date = date.AddYears(yare);
            return pc.GetYear(date) + "/" + (pc.GetMonth(date).ToString().Count() == 1 ? "0" + pc.GetMonth(date) : pc.GetMonth(date)) + "/" + (pc.GetDayOfMonth(date).ToString().Count() == 1 ? "0" + pc.GetDayOfMonth(date) : pc.GetDayOfMonth(date));

            //  return pc.GetYear(DateTime.Now.AddYears(yare)) + "/" + (pc.GetMonth(DateTime.Now.AddMonths(month)).ToString().Count() == 1 ? "0" + pc.GetMonth(DateTime.Now.AddMonths(month)) : pc.GetMonth(DateTime.Now.AddMonths(month))) + "/" + (pc.GetDayOfMonth(DateTime.Now.AddDays(day)).ToString().Count() == 1 ? "0" + pc.GetDayOfMonth(DateTime.Now.AddDays(day)).ToString() : pc.GetDayOfMonth(DateTime.Now.AddDays(day)));
        }
        public static class TimeUtils
    {
        public static long GetNanoseconds()
        {
            double timestamp = Stopwatch.GetTimestamp();
            double nanoseconds = 1_000_000_000.0 * timestamp / Stopwatch.Frequency;

            return (long)nanoseconds;
        }
        }
        public static string ID_UNIC()
        {
            long t1 = TimeUtils.GetNanoseconds();

            Thread.Sleep(1000);

            long t2 = TimeUtils.GetNanoseconds();
            long dt = t2 - t1;

            string tar = tar_fa(false);
            string tim = DateTime.Now.ToLongTimeString() + DateTime.Now.Millisecond + dt.ToString();
            return tar.Replace("/", "").Trim() + tim.Replace(":", "").Trim();
        }
        public static string toEnglishNumber(string input)
        {
            string EnglishNumbers = "";
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsDigit(input[i]))
                {
                    EnglishNumbers += char.GetNumericValue(input, i);
                }
                else
                {
                    EnglishNumbers += input[i].ToString();
                }
            }
            return EnglishNumbers;
        }
        public static string Func_random(Random random, string number)
        {

            int randomNumber = random.Next(0, 9);
            if (number.IndexOf(randomNumber.ToString()) == -1)
                number += randomNumber.ToString();

            if (number.Length < 7)
                number = Func_random(random, number);
            return number;
        }
        public static bool IsValidCodeMeli(string nationalCode)
        {
            if (!Regex.IsMatch(nationalCode, @"^\d{10}$"))
                return false;

            var check = Convert.ToInt32(nationalCode.Substring(9, 1));
            var sum = Enumerable.Range(0, 9)
                          .Select(x => Convert.ToInt32(nationalCode.Substring(x, 1)) * (10 - x))
                          .Sum() % 11;

            return sum < 2 && check == sum || sum >= 2 && check + sum == 11;
        }
        public static bool IsValidPersianText(string text)
        {
            try
            {
                return new Regex(@"[ ضصثقفغعهخحجچشسیبلاتنمکگظطزرذدئوپءآژيك]+").IsMatch(text);
            }
            catch
            {
                return false;
            }
        }
        public static string ConvertToPersian(DateTime date)
        {
            PersianCalendar pc = new PersianCalendar();
            return pc.GetYear(DateTime.Now) + "/" + (pc.GetMonth(DateTime.Now).ToString().Count() == 1 ? "0" + pc.GetMonth(DateTime.Now) : pc.GetMonth(DateTime.Now)) + "/" + (pc.GetDayOfMonth(DateTime.Now).ToString().Count() == 1 ? "0" + pc.GetDayOfMonth(DateTime.Now) : pc.GetDayOfMonth(DateTime.Now));

        }

        public static DateTime ConvertToMiladi(string date)
        {
            string[] d = new string[3];
            d = date.Split('/');
            PersianCalendar g = new PersianCalendar();
            return g.ToDateTime(int.Parse(d[0]), int.Parse(d[1]), int.Parse(d[2]), 8, 0, 0, 0);//1392/05/10
        }

        //public static PersianCalendar converttopersian(DateTime date)
        //{
        //    PersianCalendar g = new PersianCalendar();
        //    return g;
        //}
        public static string InsertComma(object o)
        {
            if (o != null)
            {
                var val = o.ToString();
                var tmp = "";
                var rst = "";
                var n = 1;

                for (var i = val.Length - 1; i >= 0; i--)
                {
                    tmp += val[i].ToString();
                    if (n % 3 == 0 && n < val.Length)
                    {
                        tmp += "/";
                    }

                    n++;
                }

                for (var i = tmp.Length - 1; i >= 0; i--)
                {
                    rst += tmp[i].ToString();
                }

                return rst;
            }

            return "";
        }

    }
}
