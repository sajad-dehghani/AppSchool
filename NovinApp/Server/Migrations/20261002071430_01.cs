using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovinApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class _01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsultantProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultantProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Report_students",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    lesson1_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson1_Percent = table.Column<float>(type: "real", nullable: false),
                    lesson1_PercentAvarage = table.Column<float>(type: "real", nullable: false),
                    lesson1_PercentMax = table.Column<float>(type: "real", nullable: false),
                    lesson1_True = table.Column<float>(type: "real", nullable: false),
                    lesson1_False = table.Column<float>(type: "real", nullable: false),
                    lesson1_Blank = table.Column<float>(type: "real", nullable: false),
                    lesson1_Taraz = table.Column<float>(type: "real", nullable: false),
                    lesson1_MaxTararz = table.Column<float>(type: "real", nullable: false),
                    lesson1_PercentMin = table.Column<float>(type: "real", nullable: false),
                    lesson1_TarazMin = table.Column<float>(type: "real", nullable: false),
                    lesson1_RankSchool = table.Column<float>(type: "real", nullable: false),
                    lesson1_RankArea = table.Column<float>(type: "real", nullable: false),
                    lesson1_RankSex = table.Column<float>(type: "real", nullable: false),
                    lesson1_RankTotal = table.Column<float>(type: "real", nullable: false),
                    lesson1_Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson2_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson2_Percent = table.Column<float>(type: "real", nullable: false),
                    lesson2_PercentAvarage = table.Column<float>(type: "real", nullable: false),
                    lesson2_PercentMax = table.Column<float>(type: "real", nullable: false),
                    lesson2_True = table.Column<float>(type: "real", nullable: false),
                    lesson2_False = table.Column<float>(type: "real", nullable: false),
                    lesson2_Blank = table.Column<float>(type: "real", nullable: false),
                    lesson2_Taraz = table.Column<float>(type: "real", nullable: false),
                    lesson2_MaxTararz = table.Column<float>(type: "real", nullable: false),
                    lesson2_PercentMin = table.Column<float>(type: "real", nullable: false),
                    lesson2_TarazMin = table.Column<float>(type: "real", nullable: false),
                    lesson2_RankSchool = table.Column<float>(type: "real", nullable: false),
                    lesson2_RankArea = table.Column<float>(type: "real", nullable: false),
                    lesson2_RankSex = table.Column<float>(type: "real", nullable: false),
                    lesson2_RankTotal = table.Column<float>(type: "real", nullable: false),
                    lesson2_Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson3_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson3_Percent = table.Column<float>(type: "real", nullable: false),
                    lesson3_PercentAvarage = table.Column<float>(type: "real", nullable: false),
                    lesson3_PercentMax = table.Column<float>(type: "real", nullable: false),
                    lesson3_True = table.Column<float>(type: "real", nullable: false),
                    lesson3_False = table.Column<float>(type: "real", nullable: false),
                    lesson3_Blank = table.Column<float>(type: "real", nullable: false),
                    lesson3_Taraz = table.Column<float>(type: "real", nullable: false),
                    lesson3_MaxTararz = table.Column<float>(type: "real", nullable: false),
                    lesson3_PercentMin = table.Column<float>(type: "real", nullable: false),
                    lesson3_TarazMin = table.Column<float>(type: "real", nullable: false),
                    lesson3_RankSchool = table.Column<float>(type: "real", nullable: false),
                    lesson3_RankArea = table.Column<float>(type: "real", nullable: false),
                    lesson3_RankSex = table.Column<float>(type: "real", nullable: false),
                    lesson3_RankTotal = table.Column<float>(type: "real", nullable: false),
                    lesson3_Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson4_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson4_Percent = table.Column<float>(type: "real", nullable: false),
                    lesson4_PercentAvarage = table.Column<float>(type: "real", nullable: false),
                    lesson4_PercentMax = table.Column<float>(type: "real", nullable: false),
                    lesson4_True = table.Column<float>(type: "real", nullable: false),
                    lesson4_False = table.Column<float>(type: "real", nullable: false),
                    lesson4_Blank = table.Column<float>(type: "real", nullable: false),
                    lesson4_Taraz = table.Column<float>(type: "real", nullable: false),
                    lesson4_MaxTararz = table.Column<float>(type: "real", nullable: false),
                    lesson4_PercentMin = table.Column<float>(type: "real", nullable: false),
                    lesson4_TarazMin = table.Column<float>(type: "real", nullable: false),
                    lesson4_RankSchool = table.Column<float>(type: "real", nullable: false),
                    lesson4_RankArea = table.Column<float>(type: "real", nullable: false),
                    lesson4_RankSex = table.Column<float>(type: "real", nullable: false),
                    lesson4_RankTotal = table.Column<float>(type: "real", nullable: false),
                    lesson4_Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson5_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson5_Percent = table.Column<float>(type: "real", nullable: false),
                    lesson5_PercentAvarage = table.Column<float>(type: "real", nullable: false),
                    lesson5_PercentMax = table.Column<float>(type: "real", nullable: false),
                    lesson5_True = table.Column<float>(type: "real", nullable: false),
                    lesson5_False = table.Column<float>(type: "real", nullable: false),
                    lesson5_Blank = table.Column<float>(type: "real", nullable: false),
                    lesson5_Taraz = table.Column<float>(type: "real", nullable: false),
                    lesson5_MaxTararz = table.Column<float>(type: "real", nullable: false),
                    lesson5_PercentMin = table.Column<float>(type: "real", nullable: false),
                    lesson5_TarazMin = table.Column<float>(type: "real", nullable: false),
                    lesson5_RankSchool = table.Column<float>(type: "real", nullable: false),
                    lesson5_RankArea = table.Column<float>(type: "real", nullable: false),
                    lesson5_RankSex = table.Column<float>(type: "real", nullable: false),
                    lesson5_RankTotal = table.Column<float>(type: "real", nullable: false),
                    lesson5_Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson6_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson6_Percent = table.Column<float>(type: "real", nullable: false),
                    lesson6_PercentAvarage = table.Column<float>(type: "real", nullable: false),
                    lesson6_PercentMax = table.Column<float>(type: "real", nullable: false),
                    lesson6_True = table.Column<float>(type: "real", nullable: false),
                    lesson6_False = table.Column<float>(type: "real", nullable: false),
                    lesson6_Blank = table.Column<float>(type: "real", nullable: false),
                    lesson6_Taraz = table.Column<float>(type: "real", nullable: false),
                    lesson6_MaxTararz = table.Column<float>(type: "real", nullable: false),
                    lesson6_PercentMin = table.Column<float>(type: "real", nullable: false),
                    lesson6_TarazMin = table.Column<float>(type: "real", nullable: false),
                    lesson6_RankSchool = table.Column<float>(type: "real", nullable: false),
                    lesson6_RankArea = table.Column<float>(type: "real", nullable: false),
                    lesson6_RankSex = table.Column<float>(type: "real", nullable: false),
                    lesson6_RankTotal = table.Column<float>(type: "real", nullable: false),
                    lesson6_Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson7_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson7_Percent = table.Column<float>(type: "real", nullable: false),
                    lesson7_PercentAvarage = table.Column<float>(type: "real", nullable: false),
                    lesson7_PercentMax = table.Column<float>(type: "real", nullable: false),
                    lesson7_True = table.Column<float>(type: "real", nullable: false),
                    lesson7_False = table.Column<float>(type: "real", nullable: false),
                    lesson7_Blank = table.Column<float>(type: "real", nullable: false),
                    lesson7_Taraz = table.Column<float>(type: "real", nullable: false),
                    lesson7_MaxTararz = table.Column<float>(type: "real", nullable: false),
                    lesson7_PercentMin = table.Column<float>(type: "real", nullable: false),
                    lesson7_TarazMin = table.Column<float>(type: "real", nullable: false),
                    lesson7_RankSchool = table.Column<float>(type: "real", nullable: false),
                    lesson7_RankArea = table.Column<float>(type: "real", nullable: false),
                    lesson7_RankSex = table.Column<float>(type: "real", nullable: false),
                    lesson7_RankTotal = table.Column<float>(type: "real", nullable: false),
                    lesson7_Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson8_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson8_Percent = table.Column<float>(type: "real", nullable: false),
                    lesson8_PercentAvarage = table.Column<float>(type: "real", nullable: false),
                    lesson8_PercentMax = table.Column<float>(type: "real", nullable: false),
                    lesson8_True = table.Column<float>(type: "real", nullable: false),
                    lesson8_False = table.Column<float>(type: "real", nullable: false),
                    lesson8_Blank = table.Column<float>(type: "real", nullable: false),
                    lesson8_Taraz = table.Column<float>(type: "real", nullable: false),
                    lesson8_MaxTararz = table.Column<float>(type: "real", nullable: false),
                    lesson8_PercentMin = table.Column<float>(type: "real", nullable: false),
                    lesson8_TarazMin = table.Column<float>(type: "real", nullable: false),
                    lesson8_RankSchool = table.Column<float>(type: "real", nullable: false),
                    lesson8_RankArea = table.Column<float>(type: "real", nullable: false),
                    lesson8_RankSex = table.Column<float>(type: "real", nullable: false),
                    lesson8_RankTotal = table.Column<float>(type: "real", nullable: false),
                    lesson8_Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson9_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson9_Percent = table.Column<float>(type: "real", nullable: false),
                    lesson9_PercentAvarage = table.Column<float>(type: "real", nullable: false),
                    lesson9_PercentMax = table.Column<float>(type: "real", nullable: false),
                    lesson9_True = table.Column<float>(type: "real", nullable: false),
                    lesson9_False = table.Column<float>(type: "real", nullable: false),
                    lesson9_Blank = table.Column<float>(type: "real", nullable: false),
                    lesson9_Taraz = table.Column<float>(type: "real", nullable: false),
                    lesson9_MaxTararz = table.Column<float>(type: "real", nullable: false),
                    lesson9_PercentMin = table.Column<float>(type: "real", nullable: false),
                    lesson9_TarazMin = table.Column<float>(type: "real", nullable: false),
                    lesson9_RankSchool = table.Column<float>(type: "real", nullable: false),
                    lesson9_RankArea = table.Column<float>(type: "real", nullable: false),
                    lesson9_RankSex = table.Column<float>(type: "real", nullable: false),
                    lesson9_RankTotal = table.Column<float>(type: "real", nullable: false),
                    lesson9_Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson10_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson10_Percent = table.Column<float>(type: "real", nullable: false),
                    lesson10_PercentAvarage = table.Column<float>(type: "real", nullable: false),
                    lesson10_PercentMax = table.Column<float>(type: "real", nullable: false),
                    lesson10_True = table.Column<float>(type: "real", nullable: false),
                    lesson10_False = table.Column<float>(type: "real", nullable: false),
                    lesson10_Blank = table.Column<float>(type: "real", nullable: false),
                    lesson10_Taraz = table.Column<float>(type: "real", nullable: false),
                    lesson10_MaxTararz = table.Column<float>(type: "real", nullable: false),
                    lesson10_PercentMin = table.Column<float>(type: "real", nullable: false),
                    lesson10_TarazMin = table.Column<float>(type: "real", nullable: false),
                    lesson10_RankSchool = table.Column<float>(type: "real", nullable: false),
                    lesson10_RankArea = table.Column<float>(type: "real", nullable: false),
                    lesson10_RankSex = table.Column<float>(type: "real", nullable: false),
                    lesson10_RankTotal = table.Column<float>(type: "real", nullable: false),
                    lesson10_Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson11_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson11_Percent = table.Column<float>(type: "real", nullable: false),
                    lesson11_PercentAvarage = table.Column<float>(type: "real", nullable: false),
                    lesson11_PercentMax = table.Column<float>(type: "real", nullable: false),
                    lesson11_True = table.Column<float>(type: "real", nullable: false),
                    lesson11_False = table.Column<float>(type: "real", nullable: false),
                    lesson11_Blank = table.Column<float>(type: "real", nullable: false),
                    lesson11_Taraz = table.Column<float>(type: "real", nullable: false),
                    lesson11_MaxTararz = table.Column<float>(type: "real", nullable: false),
                    lesson11_PercentMin = table.Column<float>(type: "real", nullable: false),
                    lesson11_TarazMin = table.Column<float>(type: "real", nullable: false),
                    lesson11_RankSchool = table.Column<float>(type: "real", nullable: false),
                    lesson11_RankArea = table.Column<float>(type: "real", nullable: false),
                    lesson11_RankSex = table.Column<float>(type: "real", nullable: false),
                    lesson11_RankTotal = table.Column<float>(type: "real", nullable: false),
                    lesson11_Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson12_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson12_Percent = table.Column<float>(type: "real", nullable: false),
                    lesson12_PercentAvarage = table.Column<float>(type: "real", nullable: false),
                    lesson12_PercentMax = table.Column<float>(type: "real", nullable: false),
                    lesson12_True = table.Column<float>(type: "real", nullable: false),
                    lesson12_False = table.Column<float>(type: "real", nullable: false),
                    lesson12_Blank = table.Column<float>(type: "real", nullable: false),
                    lesson12_Taraz = table.Column<float>(type: "real", nullable: false),
                    lesson12_MaxTararz = table.Column<float>(type: "real", nullable: false),
                    lesson12_PercentMin = table.Column<float>(type: "real", nullable: false),
                    lesson12_TarazMin = table.Column<float>(type: "real", nullable: false),
                    lesson12_RankSchool = table.Column<float>(type: "real", nullable: false),
                    lesson12_RankArea = table.Column<float>(type: "real", nullable: false),
                    lesson12_RankSex = table.Column<float>(type: "real", nullable: false),
                    lesson12_RankTotal = table.Column<float>(type: "real", nullable: false),
                    lesson12_Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson13_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson13_Percent = table.Column<float>(type: "real", nullable: false),
                    lesson13_PercentAvarage = table.Column<float>(type: "real", nullable: false),
                    lesson13_PercentMax = table.Column<float>(type: "real", nullable: false),
                    lesson13_True = table.Column<float>(type: "real", nullable: false),
                    lesson13_False = table.Column<float>(type: "real", nullable: false),
                    lesson13_Blank = table.Column<float>(type: "real", nullable: false),
                    lesson13_Taraz = table.Column<float>(type: "real", nullable: false),
                    lesson13_MaxTararz = table.Column<float>(type: "real", nullable: false),
                    lesson13_PercentMin = table.Column<float>(type: "real", nullable: false),
                    lesson13_TarazMin = table.Column<float>(type: "real", nullable: false),
                    lesson13_RankSchool = table.Column<float>(type: "real", nullable: false),
                    lesson13_RankArea = table.Column<float>(type: "real", nullable: false),
                    lesson13_RankSex = table.Column<float>(type: "real", nullable: false),
                    lesson13_RankTotal = table.Column<float>(type: "real", nullable: false),
                    lesson13_Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson14_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lesson14_Percent = table.Column<float>(type: "real", nullable: false),
                    lesson14_PercentAvarage = table.Column<float>(type: "real", nullable: false),
                    lesson14_PercentMax = table.Column<float>(type: "real", nullable: false),
                    lesson14_True = table.Column<float>(type: "real", nullable: false),
                    lesson14_False = table.Column<float>(type: "real", nullable: false),
                    lesson14_Blank = table.Column<float>(type: "real", nullable: false),
                    lesson14_Taraz = table.Column<float>(type: "real", nullable: false),
                    lesson14_MaxTararz = table.Column<float>(type: "real", nullable: false),
                    lesson14_PercentMin = table.Column<float>(type: "real", nullable: false),
                    lesson14_TarazMin = table.Column<float>(type: "real", nullable: false),
                    lesson14_RankSchool = table.Column<float>(type: "real", nullable: false),
                    lesson14_RankArea = table.Column<float>(type: "real", nullable: false),
                    lesson14_RankSex = table.Column<float>(type: "real", nullable: false),
                    lesson14_RankTotal = table.Column<float>(type: "real", nullable: false),
                    lesson14_Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Total_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Total_Percent = table.Column<float>(type: "real", nullable: false),
                    Total_PercentAvarage = table.Column<float>(type: "real", nullable: false),
                    Total_PercentMax = table.Column<float>(type: "real", nullable: false),
                    Total_True = table.Column<float>(type: "real", nullable: false),
                    Total_False = table.Column<float>(type: "real", nullable: false),
                    Total_Blank = table.Column<float>(type: "real", nullable: false),
                    Total_Taraz = table.Column<float>(type: "real", nullable: false),
                    Total_MaxTararz = table.Column<float>(type: "real", nullable: false),
                    Total_PercentMin = table.Column<float>(type: "real", nullable: false),
                    Total_TarazMin = table.Column<float>(type: "real", nullable: false),
                    Total_RankSchool = table.Column<float>(type: "real", nullable: false),
                    Total_RankArea = table.Column<float>(type: "real", nullable: false),
                    Total_RankSex = table.Column<float>(type: "real", nullable: false),
                    Total_RankTotal = table.Column<float>(type: "real", nullable: false),
                    Total_Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentCode = table.Column<long>(type: "bigint", nullable: false),
                    Student_FatherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Student_School_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Student_Region_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Student_FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Student_School_Code = table.Column<long>(type: "bigint", nullable: false),
                    Student_NationalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Student_LevelNameString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Student_Section_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pass_school = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pass_Region = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pass_Student = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnswerSheet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KeySheet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExamStatictics_PresentCount = table.Column<int>(type: "int", nullable: false),
                    SchoolStatistics_PresentCount = table.Column<int>(type: "int", nullable: false),
                    GenderStatistics_PresentCount = table.Column<int>(type: "int", nullable: false),
                    RegionStatistics_PresentCount = table.Column<int>(type: "int", nullable: false),
                    ExamName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StringName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GetWeekness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GetStrength = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadDescription = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Report_students", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SchoolCode = table.Column<long>(type: "bigint", nullable: true),
                    RegionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ManagerUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    code_meli = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    gender = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    mobile = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    pic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rool = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Id_Moshaver = table.Column<int>(type: "int", nullable: true),
                    Moshaver = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Id_School = table.Column<int>(type: "int", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    rowId = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    NationalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StudentCode = table.Column<long>(type: "bigint", nullable: true),
                    FatherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GradeLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FieldOfStudy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SchoolId = table.Column<int>(type: "int", nullable: true),
                    ConsultantId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_ConsultantProfiles_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "ConsultantProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schools_Name",
                table: "Schools",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_ConsultantId",
                table: "StudentProfiles",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_NationalCode",
                table: "StudentProfiles",
                column: "NationalCode");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_SchoolId",
                table: "StudentProfiles",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_User_active",
                table: "User",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_User_code_meli",
                table: "User",
                column: "code_meli");

            migrationBuilder.CreateIndex(
                name: "IX_User_Id_Moshaver",
                table: "User",
                column: "Id_Moshaver");

            migrationBuilder.CreateIndex(
                name: "IX_User_Id_School",
                table: "User",
                column: "Id_School");

            migrationBuilder.CreateIndex(
                name: "IX_User_Rool",
                table: "User",
                column: "Rool");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Report_students");

            migrationBuilder.DropTable(
                name: "StudentProfiles");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "ConsultantProfiles");

            migrationBuilder.DropTable(
                name: "Schools");
        }
    }
}
