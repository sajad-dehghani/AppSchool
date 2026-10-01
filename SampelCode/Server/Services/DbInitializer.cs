using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NovinApp.Server.Models;
using NovinApp.Server.MyContext;
using NovinApp.Shared.Constants;
using NovinApp.Shared.Entities;
using NovinApp.Shared.Enums;

namespace NovinApp.Server.Services
{
    public class DbInitializer : IDbInitializer
    {
        private readonly MyAppContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public DbInitializer(
            MyAppContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitializeAsync()
        {
            // ۱. اطمینان از ایجاد جداول دیتابیس
            await _context.Database.EnsureCreatedAsync();

            // ۲. ایجاد نقش‌های چهارگانه سامانه
            foreach (var roleName in UserRoles.AllRoles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new ApplicationRole(roleName, UserRoles.GetPersianTitle(roleName)));
                }
            }

            // ۳. ایجاد کاربر مدیر ارشد سیستم (SystemAdmin)
            var adminUser = await _userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    NationalCode = "0000000000",
                    FullName = "مدیر ارشد سامانه",
                    Email = "admin@novinapp.ir",
                    EmailConfirmed = true,
                    PhoneNumber = "09120000000",
                    Gender = "مرد",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, UserRoles.SystemAdmin);
                }
            }

            // ۴. ایجاد مدارس نمونه
            if (!await _context.Schools.AnyAsync())
            {
                var school1 = new School
                {
                    Name = "دبیرستان نمونه دولتی نوین",
                    SchoolCode = 1001,
                    RegionName = "منطقه ۱ تهران",
                    Address = "تهران، خیابان ولیعصر، نرسیده به میدان ونک",
                    PhoneNumber = "02188880001",
                    IsActive = true
                };

                var school2 = new School
                {
                    Name = "دبیرستان فرزانگان شاهد",
                    SchoolCode = 1002,
                    RegionName = "منطقه ۳ تهران",
                    Address = "تهران، خیابان شریعتی، بالاتر از میرداماد",
                    PhoneNumber = "02122220002",
                    IsActive = true
                };

                _context.Schools.AddRange(school1, school2);
                await _context.SaveChangesAsync();
            }

            var defaultSchool = await _context.Schools.FirstAsync();

            // ۵. ایجاد مدیر مدرسه نمونه (SchoolManager)
            var managerUser = await _userManager.FindByNameAsync("manager");
            if (managerUser == null)
            {
                managerUser = new ApplicationUser
                {
                    UserName = "manager",
                    NationalCode = "1111111111",
                    FullName = "دکتر جواد کریمی (مدیر دبیرستان نوین)",
                    PhoneNumber = "09121111111",
                    Gender = "مرد",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(managerUser, "Manager@123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(managerUser, UserRoles.SchoolManager);
                    defaultSchool.ManagerUserId = managerUser.Id;
                    await _context.SaveChangesAsync();
                }
            }

            // ۶. ایجاد مشاوران نمونه (Consultant)
            var counselor1User = await _userManager.FindByNameAsync("counselor1");
            ConsultantProfile? consultant1Profile = null;
            if (counselor1User == null)
            {
                counselor1User = new ApplicationUser
                {
                    UserName = "counselor1",
                    NationalCode = "2222222222",
                    FullName = "استاد علیرضا رضایی",
                    PhoneNumber = "09122222222",
                    Gender = "مرد",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(counselor1User, "Counselor@123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(counselor1User, UserRoles.Consultant);

                    consultant1Profile = new ConsultantProfile
                    {
                        UserId = counselor1User.Id,
                        Specialty = "مشاور ارشد کنکور تجربی و برنامه‌ریزی درسی",
                        Bio = "دارای ۱۵ سال سابقه مشاوره در مدارس برتر و هدایت تحصیلی رتبه‌های برتر کنکور",
                        IsActive = true
                    };
                    _context.ConsultantProfiles.Add(consultant1Profile);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                consultant1Profile = await _context.ConsultantProfiles.FirstOrDefaultAsync(c => c.UserId == counselor1User.Id);
            }

            var counselor2User = await _userManager.FindByNameAsync("counselor2");
            if (counselor2User == null)
            {
                counselor2User = new ApplicationUser
                {
                    UserName = "counselor2",
                    NationalCode = "3333333333",
                    FullName = "دکتر مریم احمدی",
                    PhoneNumber = "09123333333",
                    Gender = "زن",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(counselor2User, "Counselor@123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(counselor2User, UserRoles.Consultant);

                    var consultant2Profile = new ConsultantProfile
                    {
                        UserId = counselor2User.Id,
                        Specialty = "روانشناس تربیتی و مشاور استعدادیابی",
                        Bio = "متخصص آزمون‌های شخصیت‌شناسی، کتل، نئو و هدایت تحصیلی نهم به دهم",
                        IsActive = true
                    };
                    _context.ConsultantProfiles.Add(consultant2Profile);
                    await _context.SaveChangesAsync();
                }
            }

            // ۷. ایجاد دانش‌آموز نمونه (Student)
            var student1User = await _userManager.FindByNameAsync("student1");
            StudentProfile? student1Profile = null;
            if (student1User == null)
            {
                student1User = new ApplicationUser
                {
                    UserName = "student1",
                    NationalCode = "0012345678",
                    FullName = "سجاد دهقانی",
                    PhoneNumber = "09124444444",
                    Gender = "مرد",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(student1User, "Student@123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(student1User, UserRoles.Student);

                    student1Profile = new StudentProfile
                    {
                        UserId = student1User.Id,
                        NationalCode = "0012345678",
                        StudentCode = 14030101,
                        FatherName = "محمود",
                        GradeLevel = "پایه دوازدهم",
                        FieldOfStudy = "علوم تجربی",
                        SchoolId = defaultSchool.Id,
                        ConsultantId = consultant1Profile?.Id,
                        IsActive = true
                    };
                    _context.StudentProfiles.Add(student1Profile);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                student1Profile = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == student1User.Id);
            }

            // دانش‌آموز دوم نمونه
            var student2User = await _userManager.FindByNameAsync("student2");
            if (student2User == null)
            {
                student2User = new ApplicationUser
                {
                    UserName = "student2",
                    NationalCode = "0087654321",
                    FullName = "علی محمدی",
                    PhoneNumber = "09125555555",
                    Gender = "مرد",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(student2User, "Student@123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(student2User, UserRoles.Student);

                    var student2Profile = new StudentProfile
                    {
                        UserId = student2User.Id,
                        NationalCode = "0087654321",
                        StudentCode = 14030102,
                        FatherName = "رضا",
                        GradeLevel = "پایه دوازدهم",
                        FieldOfStudy = "ریاضی فیزیک",
                        SchoolId = defaultSchool.Id,
                        ConsultantId = consultant1Profile?.Id,
                        IsActive = true
                    };
                    _context.StudentProfiles.Add(student2Profile);
                    await _context.SaveChangesAsync();
                }
            }

            // ۸. ایجاد دروس پایه (Subjects)
            if (!await _context.Subjects.AnyAsync())
            {
                var subjects = new[]
                {
                    new Subject { Title = "زیست‌شناسی", Code = "BIO", FieldOfStudy = "تجربی", DisplayOrder = 1 },
                    new Subject { Title = "شیمی", Code = "CHM", FieldOfStudy = "مشترک", DisplayOrder = 2 },
                    new Subject { Title = "ریاضیات", Code = "MTH", FieldOfStudy = "مشترک", DisplayOrder = 3 },
                    new Subject { Title = "فیزیک", Code = "PHY", FieldOfStudy = "مشترک", DisplayOrder = 4 },
                    new Subject { Title = "ادبیات فارسی", Code = "LIT", FieldOfStudy = "عمومی", DisplayOrder = 5 },
                    new Subject { Title = "زبان انگلیسی", Code = "ENG", FieldOfStudy = "عمومی", DisplayOrder = 6 }
                };

                _context.Subjects.AddRange(subjects);
                await _context.SaveChangesAsync();
            }

            // ۹. ایجاد آزمون‌های نمونه و کارنامه برای دانش‌آموز نمونه
            if (!await _context.ExamsList.AnyAsync())
            {
                var exam1 = new Exam
                {
                    Title = "آزمون مرحله ۱ گزینه دو (جامع پایه)",
                    Provider = "گزینه دو",
                    StageNumber = 1,
                    AcademicYear = "۱۴۰۳-۱۴۰۴",
                    ExamDate = DateTime.Today.AddDays(-30),
                    Description = "ارزیابی مباحث پایه دهم و یازدهم"
                };

                var exam2 = new Exam
                {
                    Title = "آزمون مرحله ۲ گزینه دو (نیم‌سال اول)",
                    Provider = "گزینه دو",
                    StageNumber = 2,
                    AcademicYear = "۱۴۰۳-۱۴۰۴",
                    ExamDate = DateTime.Today.AddDays(-7),
                    Description = "ارزیابی نیم‌سال اول دوازدهم به همراه دوره پایه"
                };

                _context.ExamsList.AddRange(exam1, exam2);
                await _context.SaveChangesAsync();

                if (student1Profile != null)
                {
                    // نتیجه آزمون اول
                    var result1 = new StudentExamResult
                    {
                        StudentProfileId = student1Profile.Id,
                        ExamId = exam1.Id,
                        TotalPercent = 64.5f,
                        TotalTaraz = 6850f,
                        TotalPercentAverage = 42.0f,
                        TotalMaxTaraz = 8200f,
                        RankInSchool = 3,
                        RankInRegion = 45,
                        RankInTotal = 320,
                        StrengthsSummary = "درصد عالی در زیست‌شناسی و ادبیات",
                        WeaknessesSummary = "نیاز به مرور محاسبات استوکیومتری شیمی"
                    };

                    result1.SubjectResults.Add(new StudentExamSubjectResult { SubjectName = "زیست‌شناسی", Percent = 78.5f, Taraz = 7200f, TrueCount = 35, FalseCount = 5, BlankCount = 5, RankInSchool = 2, RankInRegion = 28, RankInTotal = 190, StatusTitle = "عالی" });
                    result1.SubjectResults.Add(new StudentExamSubjectResult { SubjectName = "شیمی", Percent = 55.0f, Taraz = 6100f, TrueCount = 20, FalseCount = 8, BlankCount = 7, RankInSchool = 8, RankInRegion = 82, RankInTotal = 560, StatusTitle = "متوسط" });
                    result1.SubjectResults.Add(new StudentExamSubjectResult { SubjectName = "ریاضیات", Percent = 60.0f, Taraz = 6600f, TrueCount = 18, FalseCount = 4, BlankCount = 8, RankInSchool = 5, RankInRegion = 60, RankInTotal = 410, StatusTitle = "خوب" });
                    result1.SubjectResults.Add(new StudentExamSubjectResult { SubjectName = "فیزیک", Percent = 65.0f, Taraz = 6900f, TrueCount = 22, FalseCount = 5, BlankCount = 3, RankInSchool = 4, RankInRegion = 40, RankInTotal = 300, StatusTitle = "خوب" });

                    // نتیجه آزمون دوم (پیشرفت)
                    var result2 = new StudentExamResult
                    {
                        StudentProfileId = student1Profile.Id,
                        ExamId = exam2.Id,
                        TotalPercent = 71.0f,
                        TotalTaraz = 7350f,
                        TotalPercentAverage = 44.5f,
                        TotalMaxTaraz = 8400f,
                        RankInSchool = 1,
                        RankInRegion = 18,
                        RankInTotal = 145,
                        StrengthsSummary = "رشد چشمگیر در شیمی و تثبیت زیست‌شناسی",
                        WeaknessesSummary = "تست‌های زمان‌دار فیزیک"
                    };

                    result2.SubjectResults.Add(new StudentExamSubjectResult { SubjectName = "زیست‌شناسی", Percent = 82.0f, Taraz = 7600f, TrueCount = 38, FalseCount = 4, BlankCount = 3, RankInSchool = 1, RankInRegion = 12, RankInTotal = 95, StatusTitle = "عالی" });
                    result2.SubjectResults.Add(new StudentExamSubjectResult { SubjectName = "شیمی", Percent = 68.0f, Taraz = 7100f, TrueCount = 25, FalseCount = 5, BlankCount = 5, RankInSchool = 2, RankInRegion = 25, RankInTotal = 180, StatusTitle = "عالی" });
                    result2.SubjectResults.Add(new StudentExamSubjectResult { SubjectName = "ریاضیات", Percent = 62.0f, Taraz = 6800f, TrueCount = 19, FalseCount = 4, BlankCount = 7, RankInSchool = 4, RankInRegion = 50, RankInTotal = 350, StatusTitle = "خوب" });
                    result2.SubjectResults.Add(new StudentExamSubjectResult { SubjectName = "فیزیک", Percent = 72.0f, Taraz = 7400f, TrueCount = 24, FalseCount = 3, BlankCount = 3, RankInSchool = 2, RankInRegion = 20, RankInTotal = 150, StatusTitle = "عالی" });

                    _context.StudentExamResults.AddRange(result1, result2);
                    await _context.SaveChangesAsync();
                }
            }

            // ۱۰. برنامه درسی نمونه برای دانش‌آموز اول
            if (student1Profile != null && !await _context.StudyPlans.AnyAsync(p => p.StudentProfileId == student1Profile.Id))
            {
                var plan = new StudyPlan
                {
                    StudentProfileId = student1Profile.Id,
                    ConsultantProfileId = consultant1Profile?.Id,
                    Title = "برنامه راهبردی هفته اول مهرماه",
                    TargetBranch = "علوم تجربی",
                    TargetWeeklyHours = 45,
                    TargetWeeklyTests = 800,
                    StartDate = DateTime.Today.AddDays(-7),
                    EndDate = DateTime.Today,
                    ConsultantNote = "تأکید روی تست‌های مفهومی زیست و محاسبات سریع شیمی",
                    IsActive = true
                };

                _context.StudyPlans.Add(plan);
                await _context.SaveChangesAsync();

                var report1 = new StudyDailyReport
                {
                    StudentProfileId = student1Profile.Id,
                    StudyPlanId = plan.Id,
                    ReportDate = DateTime.Today.AddDays(-2),
                    SubjectName = "زیست‌شناسی",
                    StudyHours = 3.5,
                    TestCount = 70,
                    CorrectCount = 58,
                    WrongCount = 12,
                    TopicsCovered = "گردش مواد و ساختار قلب",
                    StudentNote = "تست‌های ترکیبی نسبتاً چالشی بود",
                    CounselorNote = "دقت در تست‌های قیددار عالی است",
                    QualityRating = 5,
                    IsApprovedByCounselor = true
                };

                var report2 = new StudyDailyReport
                {
                    StudentProfileId = student1Profile.Id,
                    StudyPlanId = plan.Id,
                    ReportDate = DateTime.Today.AddDays(-1),
                    SubjectName = "شیمی",
                    StudyHours = 2.5,
                    TestCount = 50,
                    CorrectCount = 40,
                    WrongCount = 10,
                    TopicsCovered = "استوکیومتری و واکنش‌های شیمیایی",
                    StudentNote = "تکنیک ضرب سریع را تمرین کردم",
                    CounselorNote = "روند پیشرفت حل مسئله بسیار رضایت‌بخش است",
                    QualityRating = 4,
                    IsApprovedByCounselor = true
                };

                _context.StudyDailyReports.AddRange(report1, report2);
                await _context.SaveChangesAsync();
            }
        }
    }
}
