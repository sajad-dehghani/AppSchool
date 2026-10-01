using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovinApp.Shared
{
    public class Report_students
    {
        [Key]
        public int ID { get; set; } 
        public string? lesson1_Name { get; set; } 
        public Single lesson1_Percent { get; set; } = 0;
        public Single lesson1_PercentAvarage { get; set; } = 0;
        public Single lesson1_PercentMax { get; set; } = 0;
        public Single lesson1_True { get; set; } = 0;
        public Single lesson1_False { get; set; } = 0;
        public Single lesson1_Blank { get; set; } = 0;
        public Single lesson1_Taraz { get; set; } = 0;
        public Single lesson1_MaxTararz { get; set; } = 0;
        public Single lesson1_PercentMin { get; set; } = 0;
        public Single lesson1_TarazMin { get; set; } = 0;
        public Single lesson1_RankSchool { get; set; } = 0;
        public Single lesson1_RankArea { get; set; } = 0;
        public Single lesson1_RankSex { get; set; } = 0;
        public Single lesson1_RankTotal { get; set; } = 0;
        public string? lesson1_Status { get; set; }
        public string? lesson2_Name { get; set; } 
        public Single lesson2_Percent { get; set; } = 0;
        public Single lesson2_PercentAvarage { get; set; } = 0;
        public Single lesson2_PercentMax { get; set; } = 0;
        public Single lesson2_True { get; set; } = 0;
        public Single lesson2_False { get; set; } = 0;
        public Single lesson2_Blank { get; set; } = 0;
        public Single lesson2_Taraz { get; set; } = 0;
        public Single lesson2_MaxTararz { get; set; } = 0;
        public Single lesson2_PercentMin { get; set; } = 0;
        public Single lesson2_TarazMin { get; set; } = 0;
        public Single lesson2_RankSchool { get; set; } = 0;
        public Single lesson2_RankArea { get; set; } = 0;
        public Single lesson2_RankSex { get; set; } = 0;
        public Single lesson2_RankTotal { get; set; } = 0;
        public string? lesson2_Status { get; set; } 
        public string? lesson3_Name { get; set; }
        public Single lesson3_Percent { get; set; } = 0;
        public Single lesson3_PercentAvarage { get; set; } = 0;
        public Single lesson3_PercentMax { get; set; } = 0;
        public Single lesson3_True { get; set; } = 0;
        public Single lesson3_False { get; set; } = 0;
        public Single lesson3_Blank { get; set; } = 0;
        public Single lesson3_Taraz { get; set; } = 0;
        public Single lesson3_MaxTararz { get; set; } = 0;
        public Single lesson3_PercentMin { get; set; } = 0;
        public Single lesson3_TarazMin { get; set; } = 0;
        public Single lesson3_RankSchool { get; set; } = 0;
        public Single lesson3_RankArea { get; set; } = 0;
        public Single lesson3_RankSex { get; set; } = 0;
        public Single lesson3_RankTotal { get; set; } = 0;
        public string? lesson3_Status { get; set; }
        public string? lesson4_Name { get; set; } 
        public Single lesson4_Percent { get; set; } = 0;
        public Single lesson4_PercentAvarage { get; set; } = 0;
        public Single lesson4_PercentMax { get; set; } = 0;
        public Single lesson4_True { get; set; } = 0;
        public Single lesson4_False { get; set; } = 0;
        public Single lesson4_Blank { get; set; } = 0;
        public Single lesson4_Taraz { get; set; } = 0;
        public Single lesson4_MaxTararz { get; set; } = 0;
        public Single lesson4_PercentMin { get; set; } = 0;
        public Single lesson4_TarazMin { get; set; } = 0;
        public Single lesson4_RankSchool { get; set; } = 0;
        public Single lesson4_RankArea { get; set; } = 0;
        public Single lesson4_RankSex { get; set; } = 0;
        public Single lesson4_RankTotal { get; set; } = 0;
        public string? lesson4_Status { get; set; } 
        public string? lesson5_Name { get; set; } 
        public Single lesson5_Percent { get; set; } = 0;
        public Single lesson5_PercentAvarage { get; set; } = 0;
        public Single lesson5_PercentMax { get; set; } = 0;
        public Single lesson5_True { get; set; } = 0;
        public Single lesson5_False { get; set; } = 0;
        public Single lesson5_Blank { get; set; } = 0;
        public Single lesson5_Taraz { get; set; } = 0;
        public Single lesson5_MaxTararz { get; set; } = 0;
        public Single lesson5_PercentMin { get; set; } = 0;
        public Single lesson5_TarazMin { get; set; } = 0;
        public Single lesson5_RankSchool { get; set; } = 0;
        public Single lesson5_RankArea { get; set; } = 0;
        public Single lesson5_RankSex { get; set; } = 0;
        public Single lesson5_RankTotal { get; set; } = 0;
        public string? lesson5_Status { get; set; }
        public string? lesson6_Name { get; set; } 
        public Single lesson6_Percent { get; set; } = 0;
        public Single lesson6_PercentAvarage { get; set; } = 0;
        public Single lesson6_PercentMax { get; set; } = 0;
        public Single lesson6_True { get; set; } = 0;
        public Single lesson6_False { get; set; } = 0;
        public Single lesson6_Blank { get; set; } = 0;
        public Single lesson6_Taraz { get; set; } = 0;
        public Single lesson6_MaxTararz { get; set; } = 0;
        public Single lesson6_PercentMin { get; set; } = 0;
        public Single lesson6_TarazMin { get; set; } = 0;
        public Single lesson6_RankSchool { get; set; } = 0;
        public Single lesson6_RankArea { get; set; } = 0;
        public Single lesson6_RankSex { get; set; } = 0;
        public Single lesson6_RankTotal { get; set; } = 0;
        public string? lesson6_Status { get; set; } 
        public string? lesson7_Name { get; set; } 
        public Single lesson7_Percent { get; set; } = 0;
        public Single lesson7_PercentAvarage { get; set; } = 0;
        public Single lesson7_PercentMax { get; set; } = 0;
        public Single lesson7_True { get; set; } = 0;
        public Single lesson7_False { get; set; } = 0;
        public Single lesson7_Blank { get; set; } = 0;
        public Single lesson7_Taraz { get; set; } = 0;
        public Single lesson7_MaxTararz { get; set; } = 0;
        public Single lesson7_PercentMin { get; set; } = 0;
        public Single lesson7_TarazMin { get; set; } = 0;
        public Single lesson7_RankSchool { get; set; } = 0;
        public Single lesson7_RankArea { get; set; } = 0;
        public Single lesson7_RankSex { get; set; } = 0;
        public Single lesson7_RankTotal { get; set; } = 0;
        public string? lesson7_Status { get; set; } 
        public string? lesson8_Name { get; set; } 
        public Single lesson8_Percent { get; set; } = 0;
        public Single lesson8_PercentAvarage { get; set; } = 0;
        public Single lesson8_PercentMax { get; set; } = 0;
        public Single lesson8_True { get; set; } = 0;
        public Single lesson8_False { get; set; } = 0;
        public Single lesson8_Blank { get; set; } = 0;
        public Single lesson8_Taraz { get; set; } = 0;
        public Single lesson8_MaxTararz { get; set; } = 0;
        public Single lesson8_PercentMin { get; set; } = 0;
        public Single lesson8_TarazMin { get; set; } = 0;
        public Single lesson8_RankSchool { get; set; } = 0;
        public Single lesson8_RankArea { get; set; } = 0;
        public Single lesson8_RankSex { get; set; } = 0;
        public Single lesson8_RankTotal { get; set; } = 0;
        public string? lesson8_Status { get; set; } 
        public string? lesson9_Name { get; set; } 
        public Single lesson9_Percent { get; set; } = 0;
        public Single lesson9_PercentAvarage { get; set; } = 0;
        public Single lesson9_PercentMax { get; set; } = 0;
        public Single lesson9_True { get; set; } = 0;
        public Single lesson9_False { get; set; } = 0;
        public Single lesson9_Blank { get; set; } = 0;
        public Single lesson9_Taraz { get; set; } = 0;
        public Single lesson9_MaxTararz { get; set; } = 0;
        public Single lesson9_PercentMin { get; set; } = 0;
        public Single lesson9_TarazMin { get; set; } = 0;
        public Single lesson9_RankSchool { get; set; } = 0;
        public Single lesson9_RankArea { get; set; } = 0;
        public Single lesson9_RankSex { get; set; } = 0;
        public Single lesson9_RankTotal { get; set; } = 0;
        public string? lesson9_Status { get; set; } 
        public string? lesson10_Name { get; set; } 
        public Single lesson10_Percent { get; set; } = 0;
        public Single lesson10_PercentAvarage { get; set; } = 0;
        public Single lesson10_PercentMax { get; set; } = 0;
        public Single lesson10_True { get; set; } = 0;
        public Single lesson10_False { get; set; } = 0;
        public Single lesson10_Blank { get; set; } = 0;
        public Single lesson10_Taraz { get; set; } = 0;
        public Single lesson10_MaxTararz { get; set; } = 0;
        public Single lesson10_PercentMin { get; set; } = 0;
        public Single lesson10_TarazMin { get; set; } = 0;
        public Single lesson10_RankSchool { get; set; } = 0;
        public Single lesson10_RankArea { get; set; } = 0;
        public Single lesson10_RankSex { get; set; } = 0;
        public Single lesson10_RankTotal { get; set; } = 0;
        public string? lesson10_Status { get; set; } 
        public string? lesson11_Name { get; set; } 
        public Single lesson11_Percent { get; set; } = 0;
        public Single lesson11_PercentAvarage { get; set; } = 0;
        public Single lesson11_PercentMax { get; set; } = 0;
        public Single lesson11_True { get; set; } = 0;
        public Single lesson11_False { get; set; } = 0;
        public Single lesson11_Blank { get; set; } = 0;
        public Single lesson11_Taraz { get; set; } = 0;
        public Single lesson11_MaxTararz { get; set; } = 0;
        public Single lesson11_PercentMin { get; set; } = 0;
        public Single lesson11_TarazMin { get; set; } = 0;
        public Single lesson11_RankSchool { get; set; } = 0;
        public Single lesson11_RankArea { get; set; } = 0;
        public Single lesson11_RankSex { get; set; } = 0;
        public Single lesson11_RankTotal { get; set; } = 0;
        public string? lesson11_Status { get; set; } 
        public string? lesson12_Name { get; set; } 
        public Single lesson12_Percent { get; set; } = 0;
        public Single lesson12_PercentAvarage { get; set; } = 0;
        public Single lesson12_PercentMax { get; set; } = 0;
        public Single lesson12_True { get; set; } = 0;
        public Single lesson12_False { get; set; } = 0;
        public Single lesson12_Blank { get; set; } = 0;
        public Single lesson12_Taraz { get; set; } = 0;
        public Single lesson12_MaxTararz { get; set; } = 0;
        public Single lesson12_PercentMin { get; set; } = 0;
        public Single lesson12_TarazMin { get; set; } = 0;
        public Single lesson12_RankSchool { get; set; } = 0;
        public Single lesson12_RankArea { get; set; } = 0;
        public Single lesson12_RankSex { get; set; } = 0;
        public Single lesson12_RankTotal { get; set; } = 0;
        public string? lesson12_Status { get; set; } 

        public string? lesson13_Name { get; set; } 
        public Single lesson13_Percent { get; set; } = 0;
        public Single lesson13_PercentAvarage { get; set; } = 0;
        public Single lesson13_PercentMax { get; set; } = 0;
        public Single lesson13_True { get; set; } = 0;
        public Single lesson13_False { get; set; } = 0;
        public Single lesson13_Blank { get; set; } = 0;
        public Single lesson13_Taraz { get; set; } = 0;
        public Single lesson13_MaxTararz { get; set; } = 0;
        public Single lesson13_PercentMin { get; set; } = 0;
        public Single lesson13_TarazMin { get; set; } = 0;
        public Single lesson13_RankSchool { get; set; } = 0;
        public Single lesson13_RankArea { get; set; } = 0;
        public Single lesson13_RankSex { get; set; } = 0;
        public Single lesson13_RankTotal { get; set; } = 0;
        public string? lesson13_Status { get; set; } 
        public string? lesson14_Name { get; set; } 
        public Single lesson14_Percent { get; set; } = 0;
        public Single lesson14_PercentAvarage { get; set; } = 0;
        public Single lesson14_PercentMax { get; set; } = 0;
        public Single lesson14_True { get; set; } = 0;
        public Single lesson14_False { get; set; } = 0;
        public Single lesson14_Blank { get; set; } = 0;
        public Single lesson14_Taraz { get; set; } = 0;
        public Single lesson14_MaxTararz { get; set; } = 0;
        public Single lesson14_PercentMin { get; set; } = 0;
        public Single lesson14_TarazMin { get; set; } = 0;
        public Single lesson14_RankSchool { get; set; } = 0;
        public Single lesson14_RankArea { get; set; } = 0;
        public Single lesson14_RankSex { get; set; } = 0;
        public Single lesson14_RankTotal { get; set; } = 0;
        public string? lesson14_Status { get; set; } 
        public string? Total_Name { get; set; } 
        public Single Total_Percent { get; set; } = 0;
        public Single Total_PercentAvarage { get; set; } = 0;
        public Single Total_PercentMax { get; set; } = 0;
        public Single Total_True { get; set; } = 0;
        public Single Total_False { get; set; } = 0;
        public Single Total_Blank { get; set; } = 0;
        public Single Total_Taraz { get; set; } = 0;
        public Single Total_MaxTararz { get; set; } = 0;
        public Single Total_PercentMin { get; set; } = 0;
        public Single Total_TarazMin { get; set; } = 0;
        public Single Total_RankSchool { get; set; } = 0;
        public Single Total_RankArea { get; set; } = 0;
        public Single Total_RankSex { get; set; } = 0;
        public Single Total_RankTotal { get; set; } = 0;
        public string? Total_Status { get; set; }
        public long StudentCode { get; set; } = 0;
        public string? Student_FatherName { get; set; } 
        public string? Student_School_Name { get; set; } 
        public string? Student_Region_Name { get; set; } 
        public string? Student_FullName { get; set; }
        public long Student_School_Code { get; set; } = 0;
        public string? Student_NationalCode { get; set; }
        public string? Student_LevelNameString { get; set; } 
        public string? Student_Section_Name { get; set; } 
        public string? Pass_school { get; set; }
        public string? Pass_Region { get; set; } 
        public string? Pass_Student { get; set; }
        public string? AnswerSheet { get; set; }
        public string? KeySheet { get; set; }
        public int ExamStatictics_PresentCount { get; set; } = 0;
        public int SchoolStatistics_PresentCount { get; set; } = 0;
        public int GenderStatistics_PresentCount { get; set; } = 0;
        public int RegionStatistics_PresentCount { get; set; } = 0;
        public string? ExamName { get; set; }  = "";
        public string? StringName { get; set; }  = "";
        public string? tel { get; set; }
        public string? GetWeekness { get; set; }
        public string? GetStrength { get; set; }
        public DateTime UploadTime { get; set; }
        public string? UploadDescription { get; set; }
    }
}
