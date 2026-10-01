using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NovinApp.Server.Models;
using NovinApp.Server.MyContext;
using NovinApp.Shared.DTOs.Exams;
using NovinApp.Shared.Entities;

namespace NovinApp.Server.Services
{
    public class ExamReportService : IExamReportService
    {
        private readonly MyAppContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;
        private readonly ILogger<ExamReportService> _logger;

        public ExamReportService(
            MyAppContext context,
            UserManager<ApplicationUser> userManager,
            IAuditService auditService,
            ILogger<ExamReportService> logger)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<List<ExamDto>> GetExamsAsync(string? searchTerm = null)
        {
            var query = _context.ExamsList
                .AsNoTracking()
                .Include(e => e.Results)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(e => e.Title.Contains(term) || (e.Provider != null && e.Provider.Contains(term)));
            }

            var list = await query
                .OrderByDescending(e => e.ExamDate ?? e.CreatedAt)
                .ToListAsync();

            return list.Select(e => new ExamDto
            {
                Id = e.Id,
                Title = e.Title,
                Provider = e.Provider,
                StageNumber = e.StageNumber,
                AcademicYear = e.AcademicYear,
                ExamDate = e.ExamDate,
                Description = e.Description,
                ParticipantsCount = e.Results.Count,
                CreatedAt = e.CreatedAt
            }).ToList();
        }

        public async Task<ExamDto?> GetExamByIdAsync(int id)
        {
            var exam = await _context.ExamsList
                .AsNoTracking()
                .Include(e => e.Results)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null) return null;

            return new ExamDto
            {
                Id = exam.Id,
                Title = exam.Title,
                Provider = exam.Provider,
                StageNumber = exam.StageNumber,
                AcademicYear = exam.AcademicYear,
                ExamDate = exam.ExamDate,
                Description = exam.Description,
                ParticipantsCount = exam.Results.Count,
                CreatedAt = exam.CreatedAt
            };
        }

        public async Task<ExamDto> CreateExamAsync(CreateExamDto dto)
        {
            var exam = new Exam
            {
                Title = dto.Title.Trim(),
                Provider = dto.Provider?.Trim(),
                StageNumber = dto.StageNumber > 0 ? dto.StageNumber : 1,
                AcademicYear = dto.AcademicYear?.Trim(),
                ExamDate = dto.ExamDate,
                Description = dto.Description?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.ExamsList.Add(exam);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("CreateExam", "Exam", exam.Id.ToString(), $"ثبت آزمون جدید: {exam.Title}");

            return new ExamDto
            {
                Id = exam.Id,
                Title = exam.Title,
                Provider = exam.Provider,
                StageNumber = exam.StageNumber,
                AcademicYear = exam.AcademicYear,
                ExamDate = exam.ExamDate,
                Description = exam.Description,
                ParticipantsCount = 0,
                CreatedAt = exam.CreatedAt
            };
        }

        public async Task<ExamDto?> UpdateExamAsync(int id, UpdateExamDto dto)
        {
            var exam = await _context.ExamsList.FindAsync(id);
            if (exam == null) return null;

            exam.Title = dto.Title.Trim();
            exam.Provider = dto.Provider?.Trim();
            exam.StageNumber = dto.StageNumber;
            exam.AcademicYear = dto.AcademicYear?.Trim();
            exam.ExamDate = dto.ExamDate;
            exam.Description = dto.Description?.Trim();
            exam.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("UpdateExam", "Exam", exam.Id.ToString(), $"ویرایش مشخصات آزمون: {exam.Title}");

            var participantsCount = await _context.StudentExamResults.CountAsync(r => r.ExamId == id);

            return new ExamDto
            {
                Id = exam.Id,
                Title = exam.Title,
                Provider = exam.Provider,
                StageNumber = exam.StageNumber,
                AcademicYear = exam.AcademicYear,
                ExamDate = exam.ExamDate,
                Description = exam.Description,
                ParticipantsCount = participantsCount,
                CreatedAt = exam.CreatedAt
            };
        }

        public async Task<bool> DeleteExamAsync(int id)
        {
            var exam = await _context.ExamsList
                .Include(e => e.Results)
                .ThenInclude(r => r.SubjectResults)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null) return false;

            // حذف نتایج سابجکت‌ها و نتایج آزمون
            foreach (var r in exam.Results)
            {
                _context.StudentExamSubjectResults.RemoveRange(r.SubjectResults);
            }
            _context.StudentExamResults.RemoveRange(exam.Results);
            _context.ExamsList.Remove(exam);

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("DeleteExam", "Exam", id.ToString(), $"حذف آزمون {exam.Title} و تمام کارنامه‌های مربوطه");

            return true;
        }

        public async Task<List<ExamStudentResultItemDto>> GetExamResultsAsync(
            int examId, int? schoolId = null, int? consultantId = null, string? search = null)
        {
            var query = _context.StudentExamResults
                .AsNoTracking()
                .Where(r => r.ExamId == examId)
                .Include(r => r.StudentProfile)
                    .ThenInclude(s => s.School)
                .Include(r => r.SubjectResults)
                .AsQueryable();

            if (schoolId.HasValue)
                query = query.Where(r => r.StudentProfile.SchoolId == schoolId.Value);

            if (consultantId.HasValue)
                query = query.Where(r => r.StudentProfile.ConsultantId == consultantId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(r => r.StudentProfile.NationalCode.Contains(term));
            }

            var results = await query
                .OrderBy(r => r.RankInTotal > 0 ? r.RankInTotal : 999999)
                .ThenByDescending(r => r.TotalTaraz)
                .ToListAsync();

            var userIds = results.Select(r => r.StudentProfile.UserId).Distinct().ToList();
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            return results.Select(r =>
            {
                users.TryGetValue(r.StudentProfile.UserId, out var u);
                return new ExamStudentResultItemDto
                {
                    ResultId = r.Id,
                    StudentProfileId = r.StudentProfileId,
                    FullName = u?.FullName ?? "نامشخص",
                    NationalCode = r.StudentProfile.NationalCode,
                    SchoolName = r.StudentProfile.School?.Name,
                    TotalPercent = r.TotalPercent,
                    TotalTaraz = r.TotalTaraz,
                    RankInSchool = r.RankInSchool,
                    RankInTotal = r.RankInTotal,
                    SubjectCount = r.SubjectResults.Count
                };
            }).ToList();
        }

        public async Task<ReportCardDto?> GetStudentReportCardAsync(int examId, int studentProfileId)
        {
            var exam = await _context.ExamsList.AsNoTracking().FirstOrDefaultAsync(e => e.Id == examId);
            if (exam == null) return null;

            var student = await _context.StudentProfiles
                .AsNoTracking()
                .Include(s => s.School)
                .Include(s => s.Consultant)
                .FirstOrDefaultAsync(s => s.Id == studentProfileId);

            if (student == null) return null;

            var user = await _userManager.FindByIdAsync(student.UserId.ToString());

            string? consultantName = null;
            if (student.Consultant != null)
            {
                var cUser = await _userManager.FindByIdAsync(student.Consultant.UserId.ToString());
                consultantName = cUser?.FullName;
            }

            var result = await _context.StudentExamResults
                .AsNoTracking()
                .Include(r => r.SubjectResults)
                .FirstOrDefaultAsync(r => r.ExamId == examId && r.StudentProfileId == studentProfileId);

            if (result == null) return null;

            // شرکت‌کنندگان
            int schoolParticipants = student.SchoolId.HasValue
                ? await _context.StudentExamResults.CountAsync(r => r.ExamId == examId && r.StudentProfile.SchoolId == student.SchoolId.Value)
                : 0;
            int totalParticipants = await _context.StudentExamResults.CountAsync(r => r.ExamId == examId);

            // استخراج ریز نمرات دروس
            var subjectDtos = result.SubjectResults
                .OrderBy(sr => sr.Id)
                .Select(sr => new SubjectReportCardDto
                {
                    SubjectId = sr.SubjectId,
                    SubjectName = sr.SubjectName,
                    Percent = sr.Percent,
                    Taraz = sr.Taraz,
                    PercentAverage = sr.PercentAverage,
                    MaxTaraz = sr.MaxTaraz,
                    TrueCount = sr.TrueCount,
                    FalseCount = sr.FalseCount,
                    BlankCount = sr.BlankCount,
                    RankInSchool = sr.RankInSchool,
                    RankInRegion = sr.RankInRegion,
                    RankInTotal = sr.RankInTotal,
                    StatusTitle = sr.StatusTitle ?? (sr.Percent >= 60 ? "عالی" : (sr.Percent >= 40 ? "خوب" : "نیاز به تقویت"))
                }).ToList();

            // تحلیل هوشمند نقاط قوت و ضعف
            var strongSubjects = subjectDtos.Where(s => s.Percent >= 55).Select(s => s.SubjectName).ToList();
            var weakSubjects = subjectDtos.Where(s => s.Percent < 35).Select(s => s.SubjectName).ToList();

            // تاریخچه آزمون‌های پیشین دانش‌آموز برای نمودار ترند
            var examHistory = await _context.StudentExamResults
                .AsNoTracking()
                .Where(r => r.StudentProfileId == studentProfileId)
                .Include(r => r.Exam)
                .OrderBy(r => r.Exam.ExamDate ?? r.Exam.CreatedAt)
                .Select(r => new StudentExamHistoryItemDto
                {
                    ExamId = r.ExamId,
                    ExamTitle = r.Exam.Title,
                    ExamDate = r.Exam.ExamDate,
                    StageNumber = r.Exam.StageNumber,
                    TotalPercent = r.TotalPercent,
                    TotalTaraz = r.TotalTaraz,
                    RankInSchool = r.RankInSchool,
                    RankInTotal = r.RankInTotal
                }).ToListAsync();

            return new ReportCardDto
            {
                ExamId = exam.Id,
                ExamTitle = exam.Title,
                Provider = exam.Provider,
                StageNumber = exam.StageNumber,
                ExamDate = exam.ExamDate,

                StudentProfileId = student.Id,
                FullName = user?.FullName ?? "نامشخص",
                NationalCode = student.NationalCode,
                StudentNumber = student.StudentCode?.ToString(),
                SchoolName = student.School?.Name,
                GradeLevel = student.GradeLevel,
                Major = student.FieldOfStudy,
                ConsultantName = consultantName,

                TotalPercent = result.TotalPercent,
                TotalTaraz = result.TotalTaraz,
                TotalPercentAverage = result.TotalPercentAverage,
                TotalMaxTaraz = result.TotalMaxTaraz,
                RankInSchool = result.RankInSchool,
                RankInRegion = result.RankInRegion,
                RankInTotal = result.RankInTotal,
                TotalParticipantsSchool = schoolParticipants,
                TotalParticipantsOverall = totalParticipants,

                StrengthsSummary = result.StrengthsSummary,
                WeaknessesSummary = result.WeaknessesSummary,
                StrongSubjects = strongSubjects,
                WeakSubjects = weakSubjects,

                Subjects = subjectDtos,
                ExamHistory = examHistory
            };
        }

        public async Task<ReportCardDto?> GetStudentReportCardByNationalCodeAsync(int examId, string nationalCode)
        {
            var student = await _context.StudentProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.NationalCode == nationalCode.Trim());

            if (student == null) return null;

            return await GetStudentReportCardAsync(examId, student.Id);
        }

        public async Task<List<StudentExamHistoryItemDto>> GetStudentExamHistoryAsync(int studentProfileId)
        {
            return await _context.StudentExamResults
                .AsNoTracking()
                .Where(r => r.StudentProfileId == studentProfileId)
                .Include(r => r.Exam)
                .OrderBy(r => r.Exam.ExamDate ?? r.Exam.CreatedAt)
                .Select(r => new StudentExamHistoryItemDto
                {
                    ExamId = r.ExamId,
                    ExamTitle = r.Exam.Title,
                    ExamDate = r.Exam.ExamDate,
                    StageNumber = r.Exam.StageNumber,
                    TotalPercent = r.TotalPercent,
                    TotalTaraz = r.TotalTaraz,
                    RankInSchool = r.RankInSchool,
                    RankInTotal = r.RankInTotal
                }).ToListAsync();
        }

        public async Task<ExamAnalyticsDto?> GetExamAnalyticsAsync(int examId)
        {
            var exam = await _context.ExamsList.AsNoTracking().FirstOrDefaultAsync(e => e.Id == examId);
            if (exam == null) return null;

            var results = await _context.StudentExamResults
                .AsNoTracking()
                .Where(r => r.ExamId == examId)
                .Include(r => r.StudentProfile)
                    .ThenInclude(s => s.School)
                .Include(r => r.SubjectResults)
                .ToListAsync();

            if (!results.Any())
            {
                return new ExamAnalyticsDto
                {
                    ExamId = exam.Id,
                    ExamTitle = exam.Title,
                    TotalParticipants = 0
                };
            }

            var userIds = results.Select(r => r.StudentProfile.UserId).Distinct().ToList();
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            // تحلیل دروس
            var allSubjectResults = results.SelectMany(r => r.SubjectResults).ToList();
            var subjectGroups = allSubjectResults.GroupBy(sr => sr.SubjectName);

            var subjectAnalytics = subjectGroups.Select(g => new SubjectAnalyticsDto
            {
                SubjectName = g.Key,
                AveragePercent = (float)Math.Round(g.Average(s => s.Percent), 1),
                MaxPercent = (float)Math.Round(g.Max(s => s.Percent), 1),
                MinPercent = (float)Math.Round(g.Min(s => s.Percent), 1)
            }).OrderByDescending(s => s.AveragePercent).ToList();

            // رتبه‌های برتر
            var topStudents = results
                .OrderBy(r => r.RankInTotal > 0 ? r.RankInTotal : 999999)
                .ThenByDescending(r => r.TotalTaraz)
                .Take(10)
                .Select(r =>
                {
                    users.TryGetValue(r.StudentProfile.UserId, out var u);
                    return new TopStudentDto
                    {
                        StudentProfileId = r.StudentProfileId,
                        FullName = u?.FullName ?? "نامشخص",
                        NationalCode = r.StudentProfile.NationalCode,
                        SchoolName = r.StudentProfile.School?.Name,
                        TotalPercent = r.TotalPercent,
                        TotalTaraz = r.TotalTaraz,
                        RankInTotal = r.RankInTotal
                    };
                }).ToList();

            return new ExamAnalyticsDto
            {
                ExamId = exam.Id,
                ExamTitle = exam.Title,
                TotalParticipants = results.Count,
                AveragePercent = (float)Math.Round(results.Average(r => r.TotalPercent), 1),
                MaxPercent = (float)Math.Round(results.Max(r => r.TotalPercent), 1),
                MinPercent = (float)Math.Round(results.Min(r => r.TotalPercent), 1),
                AverageTaraz = (float)Math.Round(results.Average(r => r.TotalTaraz), 1),
                MaxTaraz = (float)Math.Round(results.Max(r => r.TotalTaraz), 1),
                Subjects = subjectAnalytics,
                TopStudents = topStudents
            };
        }

        public async Task<bool> CalculateRanksAndAveragesAsync(int examId)
        {
            var results = await _context.StudentExamResults
                .Include(r => r.StudentProfile)
                .Include(r => r.SubjectResults)
                .Where(r => r.ExamId == examId)
                .ToListAsync();

            if (!results.Any()) return false;

            // ۱. میانگین و بالاترین تراز کل
            float avgPercent = results.Average(r => r.TotalPercent);
            float maxTaraz = results.Max(r => r.TotalTaraz);

            // ۲. رتبه کل بر اساس تراز (یا درصد اگر تراز صفر بود)
            var sortedTotal = results
                .OrderByDescending(r => r.TotalTaraz > 0 ? r.TotalTaraz : r.TotalPercent)
                .ToList();

            for (int i = 0; i < sortedTotal.Count; i++)
            {
                sortedTotal[i].RankInTotal = i + 1;
                sortedTotal[i].TotalPercentAverage = (float)Math.Round(avgPercent, 1);
                sortedTotal[i].TotalMaxTaraz = (float)Math.Round(maxTaraz, 1);
            }

            // ۳. رتبه در مدرسه
            var bySchool = results
                .Where(r => r.StudentProfile.SchoolId.HasValue)
                .GroupBy(r => r.StudentProfile.SchoolId!.Value);

            foreach (var schoolGroup in bySchool)
            {
                var sortedSchool = schoolGroup
                    .OrderByDescending(r => r.TotalTaraz > 0 ? r.TotalTaraz : r.TotalPercent)
                    .ToList();

                for (int i = 0; i < sortedSchool.Count; i++)
                {
                    sortedSchool[i].RankInSchool = i + 1;
                }
            }

            // ۴. محاسبات برای دروس
            var allSubjectResults = results.SelectMany(r => r.SubjectResults).ToList();
            var subjectGroups = allSubjectResults.GroupBy(sr => sr.SubjectName);

            foreach (var sGroup in subjectGroups)
            {
                float subjAvg = sGroup.Average(sr => sr.Percent);
                float subjMax = sGroup.Max(sr => sr.Percent);
                float subjMaxTaraz = sGroup.Max(sr => sr.Taraz);

                // رتبه کل درس
                var sortedSubj = sGroup
                    .OrderByDescending(sr => sr.Taraz > 0 ? sr.Taraz : sr.Percent)
                    .ToList();

                for (int i = 0; i < sortedSubj.Count; i++)
                {
                    sortedSubj[i].RankInTotal = i + 1;
                    sortedSubj[i].PercentAverage = (float)Math.Round(subjAvg, 1);
                    sortedSubj[i].MaxTaraz = (float)Math.Round(subjMaxTaraz > 0 ? subjMaxTaraz : subjMax, 1);

                    // وضعیت کیفی درس
                    sortedSubj[i].StatusTitle = sortedSubj[i].Percent switch
                    {
                        >= 75 => "عالی",
                        >= 55 => "خوب",
                        >= 35 => "متوسط",
                        _ => "نیاز به تقویت"
                    };
                }

                // رتبه در مدرسه برای درس
                var subjBySchool = sGroup
                    .Where(sr => sr.StudentExamResult.StudentProfile.SchoolId.HasValue)
                    .GroupBy(sr => sr.StudentExamResult.StudentProfile.SchoolId!.Value);

                foreach (var schoolSubjGroup in subjBySchool)
                {
                    var sortedSchoolSubj = schoolSubjGroup
                        .OrderByDescending(sr => sr.Taraz > 0 ? sr.Taraz : sr.Percent)
                        .ToList();

                    for (int i = 0; i < sortedSchoolSubj.Count; i++)
                    {
                        sortedSchoolSubj[i].RankInSchool = i + 1;
                    }
                }
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("CalculateRanks", "Exam", examId.ToString(),
                $"محاسبه مجدد رتبه‌ها و میانگین‌ها برای {results.Count} کارنامه در آزمون {examId}");

            return true;
        }
    }
}
