using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovinApp.Server.MyContext;
using NovinApp.Server.Services;
using NovinApp.Shared.Constants;
using NovinApp.Shared.DTOs.Common;
using NovinApp.Shared.DTOs.StudyPlans;
using NovinApp.Shared.Entities;
using NovinApp.Shared.Enums;
using NovinApp.Shared.Helpers;

namespace NovinApp.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudyPlansController : ControllerBase
    {
        private readonly MyAppContext _context;
        private readonly ICurrentUserService _currentUserService;

        public StudyPlansController(MyAppContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        // ==========================================
        // ۱. دریافت لیست برنامه‌ها با فیلترهای مختلف
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<StudyPlanItemDto>>>> GetPlans(
            [FromQuery] int? studentUserId = null,
            [FromQuery] int? consultantUserId = null,
            [FromQuery] string? persianDate = null,
            [FromQuery] string? startPersianDate = null,
            [FromQuery] string? endPersianDate = null,
            [FromQuery] string? activityType = null)
        {
            try
            {
                var query = _context.StudyPlanItems.AsNoTracking().AsQueryable();

                // کنترل دسترسی بر اساس نقش
                if (_currentUserService.IsStudent)
                {
                    var currentStudentId = _currentUserService.UserId ?? 0;
                    query = query.Where(p => p.StudentUserId == currentStudentId);
                }
                else if (_currentUserService.IsConsultant && !_currentUserService.IsSystemAdmin)
                {
                    var currentConsId = _currentUserService.UserId ?? 0;
                    // مشاور برنامه‌هایی که خودش گذاشته یا برای دانش‌آموزانش هست را می‌بیند
                    query = query.Where(p => p.ConsultantUserId == currentConsId || 
                        _context.User.Any(u => u.Id == p.StudentUserId && u.Id_Moshaver == currentConsId));

                    if (studentUserId.HasValue && studentUserId.Value > 0)
                    {
                        query = query.Where(p => p.StudentUserId == studentUserId.Value);
                    }
                }
                else if (_currentUserService.IsSchoolManager && !_currentUserService.IsSystemAdmin)
                {
                    var schoolId = _currentUserService.SchoolId;
                    if (schoolId.HasValue)
                    {
                        query = query.Where(p => p.SchoolId == schoolId.Value ||
                            _context.User.Any(u => u.Id == p.StudentUserId && u.Id_School == schoolId.Value));
                    }
                    if (studentUserId.HasValue) query = query.Where(p => p.StudentUserId == studentUserId.Value);
                }
                else
                {
                    // ادمین
                    if (studentUserId.HasValue && studentUserId.Value > 0) query = query.Where(p => p.StudentUserId == studentUserId.Value);
                    if (consultantUserId.HasValue && consultantUserId.Value > 0) query = query.Where(p => p.ConsultantUserId == consultantUserId.Value);
                }

                // فیلترهای تاریخی و موضوعی
                if (!string.IsNullOrWhiteSpace(persianDate))
                {
                    query = query.Where(p => p.PersianDate == persianDate);
                }

                if (!string.IsNullOrWhiteSpace(startPersianDate))
                {
                    query = query.Where(p => string.Compare(p.PersianDate, startPersianDate) >= 0);
                }

                if (!string.IsNullOrWhiteSpace(endPersianDate))
                {
                    query = query.Where(p => string.Compare(p.PersianDate, endPersianDate) <= 0);
                }

                if (!string.IsNullOrWhiteSpace(activityType))
                {
                    query = query.Where(p => p.ActivityType == activityType);
                }

                var items = await query
                    .OrderBy(p => p.PersianDate)
                    .ThenBy(p => p.StartTime)
                    .Select(p => new StudyPlanItemDto
                    {
                        Id = p.Id,
                        ConsultantUserId = p.ConsultantUserId,
                        ConsultantName = p.ConsultantName,
                        StudentUserId = p.StudentUserId,
                        StudentName = p.StudentName,
                        SchoolId = p.SchoolId,
                        PersianDate = p.PersianDate,
                        GregorianDate = p.GregorianDate,
                        StartTime = p.StartTime,
                        DurationMinutes = p.DurationMinutes,
                        ActivityType = p.ActivityType,
                        TestCount = p.TestCount,
                        TestType = p.TestType,
                        GradeLevel = p.GradeLevel,
                        FieldOfStudy = p.FieldOfStudy,
                        Subject = p.Subject,
                        Chapter = p.Chapter,
                        Topic = p.Topic,
                        ConsultantNote = p.ConsultantNote,
                        ConsultantAttachmentUrl = p.ConsultantAttachmentUrl,
                        ConsultantAttachmentName = p.ConsultantAttachmentName,
                        IsCounselingSession = p.IsCounselingSession,
                        CounselingType = p.CounselingType,
                        MeetingLink = p.MeetingLink,
                        Status = p.Status,
                        IsFeedbackSubmitted = p.IsFeedbackSubmitted,
                        FeedbackSubmittedAt = p.FeedbackSubmittedAt,
                        StudentStatus = p.StudentStatus,
                        QualityRating = p.QualityRating,
                        QualityText = p.QualityText,
                        StudyPercentage = p.StudyPercentage,
                        CorrectTests = p.CorrectTests,
                        WrongTests = p.WrongTests,
                        UnansweredTests = p.UnansweredTests,
                        ActualDurationMinutes = p.ActualDurationMinutes,
                        StudentNote = p.StudentNote,
                        StudentAttachmentUrl = p.StudentAttachmentUrl,
                        StudentAttachmentName = p.StudentAttachmentName,
                        TestSource = p.TestSource
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<StudyPlanItemDto>>.SuccessResult(items));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<List<StudyPlanItemDto>>.FailureResult($"خطا در دریافت لیست برنامه‌ها: {ex.Message}"));
            }
        }

        // ==========================================
        // ۲. دریافت یک برنامه بر اساس شناسه
        // ==========================================
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<StudyPlanItemDto>>> GetById(int id)
        {
            var p = await _context.StudyPlanItems.FindAsync(id);
            if (p == null) return Ok(ApiResponse<StudyPlanItemDto>.FailureResult("برنامه یافت نشد."));

            var dto = new StudyPlanItemDto
            {
                Id = p.Id,
                ConsultantUserId = p.ConsultantUserId,
                ConsultantName = p.ConsultantName,
                StudentUserId = p.StudentUserId,
                StudentName = p.StudentName,
                SchoolId = p.SchoolId,
                PersianDate = p.PersianDate,
                GregorianDate = p.GregorianDate,
                StartTime = p.StartTime,
                DurationMinutes = p.DurationMinutes,
                ActivityType = p.ActivityType,
                TestCount = p.TestCount,
                TestType = p.TestType,
                GradeLevel = p.GradeLevel,
                FieldOfStudy = p.FieldOfStudy,
                Subject = p.Subject,
                Chapter = p.Chapter,
                Topic = p.Topic,
                ConsultantNote = p.ConsultantNote,
                ConsultantAttachmentUrl = p.ConsultantAttachmentUrl,
                ConsultantAttachmentName = p.ConsultantAttachmentName,
                IsCounselingSession = p.IsCounselingSession,
                CounselingType = p.CounselingType,
                MeetingLink = p.MeetingLink,
                Status = p.Status,
                IsFeedbackSubmitted = p.IsFeedbackSubmitted,
                FeedbackSubmittedAt = p.FeedbackSubmittedAt,
                StudentStatus = p.StudentStatus,
                QualityRating = p.QualityRating,
                QualityText = p.QualityText,
                StudyPercentage = p.StudyPercentage,
                CorrectTests = p.CorrectTests,
                WrongTests = p.WrongTests,
                UnansweredTests = p.UnansweredTests,
                ActualDurationMinutes = p.ActualDurationMinutes,
                StudentNote = p.StudentNote,
                StudentAttachmentUrl = p.StudentAttachmentUrl,
                StudentAttachmentName = p.StudentAttachmentName,
                TestSource = p.TestSource
            };

            return Ok(ApiResponse<StudyPlanItemDto>.SuccessResult(dto));
        }

        // ==========================================
        // ۳. ایجاد برنامه جدید توسط مشاور یا کاربر مجاز
        // ==========================================
        [HttpPost]
        public async Task<ActionResult<ApiResponse<StudyPlanItemDto>>> Create([FromBody] StudyPlanCreateUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Ok(ApiResponse<StudyPlanItemDto>.FailureResult("اطلاعات ورودی معتبر نمی‌باشد."));

                // یافتن اطلاعات دانش‌آموز
                var student = await _context.User.FindAsync(dto.StudentUserId);
                if (student == null)
                    return Ok(ApiResponse<StudyPlanItemDto>.FailureResult("دانش‌آموز انتخاب‌شده یافت نشد."));

                var currentUserId = _currentUserService.UserId ?? 0;
                var consultant = await _context.User.FindAsync(currentUserId);

                // تبدیل تاریخ شمسی به میلادی
                DateTime gDate = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(dto.PersianDate))
                {
                    if (PersianDateHelper.TryParsePersianDate(dto.PersianDate, out var parsed))
                    {
                        gDate = parsed;
                    }
                }

                var entity = new StudyPlanItem
                {
                    ConsultantUserId = currentUserId,
                    ConsultantName = consultant?.fname ?? _currentUserService.FullName ?? "مشاور",
                    StudentUserId = dto.StudentUserId,
                    StudentName = student.fname,
                    SchoolId = student.Id_School,
                    PersianDate = dto.PersianDate,
                    GregorianDate = gDate,
                    StartTime = dto.StartTime ?? "08:00",
                    DurationMinutes = dto.DurationMinutes,
                    ActivityType = dto.ActivityType,
                    TestCount = dto.TestCount,
                    TestType = dto.TestType,
                    GradeLevel = dto.GradeLevel,
                    FieldOfStudy = dto.FieldOfStudy,
                    Subject = dto.Subject,
                    Chapter = dto.Chapter,
                    Topic = dto.Topic,
                    ConsultantNote = dto.ConsultantNote,
                    ConsultantAttachmentUrl = dto.ConsultantAttachmentUrl,
                    ConsultantAttachmentName = dto.ConsultantAttachmentName,
                    IsCounselingSession = dto.IsCounselingSession || dto.ActivityType == ActivityTypes.InPersonCounseling || dto.ActivityType == ActivityTypes.OnlineCounseling,
                    CounselingType = dto.CounselingType ?? (dto.ActivityType == ActivityTypes.InPersonCounseling ? "حضوری" : (dto.ActivityType == ActivityTypes.OnlineCounseling ? "آنلاین" : null)),
                    MeetingLink = dto.MeetingLink,
                    Status = StudyTaskStatus.Planned,
                    CreatedAt = DateTime.Now
                };

                await _context.StudyPlanItems.AddAsync(entity);
                await _context.SaveChangesAsync();

                var resultDto = new StudyPlanItemDto
                {
                    Id = entity.Id,
                    ConsultantUserId = entity.ConsultantUserId,
                    ConsultantName = entity.ConsultantName,
                    StudentUserId = entity.StudentUserId,
                    StudentName = entity.StudentName,
                    PersianDate = entity.PersianDate,
                    StartTime = entity.StartTime,
                    DurationMinutes = entity.DurationMinutes,
                    ActivityType = entity.ActivityType,
                    TestCount = entity.TestCount,
                    TestType = entity.TestType,
                    GradeLevel = entity.GradeLevel,
                    FieldOfStudy = entity.FieldOfStudy,
                    Subject = entity.Subject,
                    Chapter = entity.Chapter,
                    Topic = entity.Topic,
                    ConsultantNote = entity.ConsultantNote,
                    ConsultantAttachmentUrl = entity.ConsultantAttachmentUrl,
                    IsCounselingSession = entity.IsCounselingSession,
                    CounselingType = entity.CounselingType,
                    MeetingLink = entity.MeetingLink,
                    Status = entity.Status
                };

                return Ok(ApiResponse<StudyPlanItemDto>.SuccessResult(resultDto, "برنامه با موفقیت ثبت شد."));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<StudyPlanItemDto>.FailureResult($"خطا در ثبت برنامه: {ex.Message}"));
            }
        }

        // ==========================================
        // ۴. ویرایش برنامه توسط مشاور
        // ==========================================
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<StudyPlanItemDto>>> Update(int id, [FromBody] StudyPlanCreateUpdateDto dto)
        {
            try
            {
                var entity = await _context.StudyPlanItems.FindAsync(id);
                if (entity == null) return Ok(ApiResponse<StudyPlanItemDto>.FailureResult("برنامه یافت نشد."));

                // اگر دانش‌آموز تغییر کرده باشد
                if (entity.StudentUserId != dto.StudentUserId)
                {
                    var student = await _context.User.FindAsync(dto.StudentUserId);
                    if (student != null)
                    {
                        entity.StudentUserId = dto.StudentUserId;
                        entity.StudentName = student.fname;
                        entity.SchoolId = student.Id_School;
                    }
                }

                if (!string.IsNullOrWhiteSpace(dto.PersianDate))
                {
                    entity.PersianDate = dto.PersianDate;
                    if (PersianDateHelper.TryParsePersianDate(dto.PersianDate, out var parsed))
                    {
                        entity.GregorianDate = parsed;
                    }
                }

                entity.StartTime = dto.StartTime ?? entity.StartTime;
                entity.DurationMinutes = dto.DurationMinutes;
                entity.ActivityType = dto.ActivityType;
                entity.TestCount = dto.TestCount;
                entity.TestType = dto.TestType;
                entity.GradeLevel = dto.GradeLevel;
                entity.FieldOfStudy = dto.FieldOfStudy;
                entity.Subject = dto.Subject;
                entity.Chapter = dto.Chapter;
                entity.Topic = dto.Topic;
                entity.ConsultantNote = dto.ConsultantNote;
                entity.ConsultantAttachmentUrl = dto.ConsultantAttachmentUrl;
                entity.ConsultantAttachmentName = dto.ConsultantAttachmentName;
                entity.IsCounselingSession = dto.IsCounselingSession || dto.ActivityType == ActivityTypes.InPersonCounseling || dto.ActivityType == ActivityTypes.OnlineCounseling;
                entity.CounselingType = dto.CounselingType ?? (dto.ActivityType == ActivityTypes.InPersonCounseling ? "حضوری" : (dto.ActivityType == ActivityTypes.OnlineCounseling ? "آنلاین" : null));
                entity.MeetingLink = dto.MeetingLink;
                entity.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return Ok(ApiResponse<StudyPlanItemDto>.SuccessResult(null, "برنامه با موفقیت ویرایش شد."));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<StudyPlanItemDto>.FailureResult($"خطا در ویرایش برنامه: {ex.Message}"));
            }
        }

        // ==========================================
        // ۵. حذف برنامه
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var entity = await _context.StudyPlanItems.FindAsync(id);
                if (entity == null) return Ok(ApiResponse<bool>.FailureResult("برنامه یافت نشد."));

                _context.StudyPlanItems.Remove(entity);
                await _context.SaveChangesAsync();
                return Ok(ApiResponse<bool>.SuccessResult(true, "برنامه با موفقیت حذف گردید."));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<bool>.FailureResult($"خطا در حذف برنامه: {ex.Message}"));
            }
        }

        // ==========================================
        // ۶. ثبت بازخورد و گزارش کار توسط دانش‌آموز
        // ==========================================
        [HttpPost("{id}/feedback")]
        public async Task<ActionResult<ApiResponse<bool>>> SubmitFeedback(int id, [FromBody] StudyFeedbackSubmitDto dto)
        {
            try
            {
                var entity = await _context.StudyPlanItems.FindAsync(id);
                if (entity == null) return Ok(ApiResponse<bool>.FailureResult("برنامه یافت نشد."));

                // ثبت بازخورد دانش‌آموز
                entity.IsFeedbackSubmitted = true;
                entity.FeedbackSubmittedAt = DateTime.Now;
                entity.StudentStatus = dto.StudentStatus;
                entity.QualityRating = dto.QualityRating;
                entity.QualityText = QualityRatings.GetLabel(dto.QualityRating);
                entity.StudyPercentage = dto.StudyPercentage;
                entity.CorrectTests = dto.CorrectTests;
                entity.WrongTests = dto.WrongTests;
                entity.UnansweredTests = dto.UnansweredTests;
                entity.ActualDurationMinutes = dto.ActualDurationMinutes;
                entity.StudentNote = dto.StudentNote;
                entity.StudentAttachmentUrl = dto.StudentAttachmentUrl;
                entity.StudentAttachmentName = dto.StudentAttachmentName;
                entity.TestSource = dto.TestSource;

                // به‌روزرسانی وضعیت کلی برنامه
                if (dto.StudentStatus == "انجام کامل" || dto.StudentStatus == "انجام شد" || dto.StudyPercentage >= 90)
                {
                    entity.Status = StudyTaskStatus.Completed;
                }
                else if (dto.StudentStatus == "ناقص" || (dto.StudyPercentage > 0 && dto.StudyPercentage < 90))
                {
                    entity.Status = StudyTaskStatus.Incomplete;
                }
                else if (dto.StudentStatus == "انجام نشد")
                {
                    entity.Status = StudyTaskStatus.Skipped;
                }

                entity.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResult(true, "بازخورد و گزارش مطالعه شما با موفقیت ثبت گردید."));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<bool>.FailureResult($"خطا در ثبت بازخورد: {ex.Message}"));
            }
        }

        // ==========================================
        // ۷. آمار عملکرد تحصیلی و برنامه‌ریزی
        // ==========================================
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<StudyPlannerStatsDto>>> GetStats(
            [FromQuery] int? studentUserId = null,
            [FromQuery] string? startPersianDate = null,
            [FromQuery] string? endPersianDate = null)
        {
            try
            {
                var query = _context.StudyPlanItems.AsNoTracking().AsQueryable();

                if (_currentUserService.IsStudent)
                {
                    var sid = _currentUserService.UserId ?? 0;
                    query = query.Where(p => p.StudentUserId == sid);
                }
                else if (studentUserId.HasValue && studentUserId.Value > 0)
                {
                    query = query.Where(p => p.StudentUserId == studentUserId.Value);
                }
                else if (_currentUserService.IsConsultant && !_currentUserService.IsSystemAdmin)
                {
                    var cid = _currentUserService.UserId ?? 0;
                    query = query.Where(p => p.ConsultantUserId == cid);
                }

                if (!string.IsNullOrWhiteSpace(startPersianDate))
                    query = query.Where(p => string.Compare(p.PersianDate, startPersianDate) >= 0);

                if (!string.IsNullOrWhiteSpace(endPersianDate))
                    query = query.Where(p => string.Compare(p.PersianDate, endPersianDate) <= 0);

                var list = await query.ToListAsync();

                var stats = new StudyPlannerStatsDto
                {
                    TotalPlans = list.Count,
                    CompletedPlans = list.Count(p => p.Status == StudyTaskStatus.Completed),
                    IncompletePlans = list.Count(p => p.Status == StudyTaskStatus.Incomplete),
                    SkippedPlans = list.Count(p => p.Status == StudyTaskStatus.Skipped),
                    PlannedTotalHours = Math.Round(list.Sum(p => (double)p.DurationMinutes) / 60.0, 1),
                    ActualCompletedHours = Math.Round(list.Where(p => p.ActualDurationMinutes.HasValue).Sum(p => (double)p.ActualDurationMinutes!.Value) / 60.0, 1),
                    TotalPlannedTests = list.Sum(p => p.TestCount ?? 0),
                    TotalCorrectTests = list.Sum(p => p.CorrectTests ?? 0),
                    TotalWrongTests = list.Sum(p => p.WrongTests ?? 0),
                    TotalUnansweredTests = list.Sum(p => p.UnansweredTests ?? 0),
                };

                stats.CompletionRate = stats.TotalPlans > 0 
                    ? Math.Round((double)stats.CompletedPlans / stats.TotalPlans * 100, 1) 
                    : 0;

                int totalDoneTests = stats.TotalCorrectTests + stats.TotalWrongTests + stats.TotalUnansweredTests;
                stats.TestAccuracyRate = totalDoneTests > 0 
                    ? Math.Round((double)stats.TotalCorrectTests / totalDoneTests * 100, 1) 
                    : 0;

                return Ok(ApiResponse<StudyPlannerStatsDto>.SuccessResult(stats));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<StudyPlannerStatsDto>.FailureResult($"خطا در دریافت آمار: {ex.Message}"));
            }
        }

        // ==========================================
        // ۸. مدیریت بانک مباحث و سرفصل‌های دروس
        // ==========================================
        [HttpGet("topics")]
        public async Task<ActionResult<ApiResponse<List<StudySubjectTopicDto>>>> GetTopics(
            [FromQuery] string? gradeLevel = null,
            [FromQuery] string? fieldOfStudy = null,
            [FromQuery] string? subject = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.StudySubjectTopics.AsNoTracking().Where(t => t.IsActive);

                if (!string.IsNullOrWhiteSpace(gradeLevel))
                    query = query.Where(t => t.GradeLevel == gradeLevel);

                if (!string.IsNullOrWhiteSpace(fieldOfStudy))
                    query = query.Where(t => t.FieldOfStudy == fieldOfStudy || t.FieldOfStudy == "عمومی / متوسطه اول" || t.FieldOfStudy == "عمومی");

                if (!string.IsNullOrWhiteSpace(subject))
                    query = query.Where(t => t.Subject == subject);

                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(t => t.Topic.Contains(search) || t.Chapter.Contains(search) || t.Subject.Contains(search));

                var list = await query
                    .OrderBy(t => t.Subject)
                    .ThenBy(t => t.Chapter)
                    .ThenBy(t => t.PriorityOrder)
                    .Select(t => new StudySubjectTopicDto
                    {
                        Id = t.Id,
                        GradeLevel = t.GradeLevel,
                        FieldOfStudy = t.FieldOfStudy,
                        Subject = t.Subject,
                        Chapter = t.Chapter,
                        Topic = t.Topic,
                        Description = t.Description,
                        PriorityOrder = t.PriorityOrder,
                        IsActive = t.IsActive
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<StudySubjectTopicDto>>.SuccessResult(list));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<List<StudySubjectTopicDto>>.FailureResult($"خطا در دریافت مباحث: {ex.Message}"));
            }
        }

        [HttpPost("topics")]
        public async Task<ActionResult<ApiResponse<StudySubjectTopicDto>>> AddTopic([FromBody] StudySubjectTopicDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Subject) || string.IsNullOrWhiteSpace(dto.Topic))
                    return Ok(ApiResponse<StudySubjectTopicDto>.FailureResult("نام درس و مبحث الزامی است."));

                var entity = new StudySubjectTopic
                {
                    GradeLevel = dto.GradeLevel ?? "نامشخص",
                    FieldOfStudy = dto.FieldOfStudy ?? "عمومی",
                    Subject = dto.Subject.Trim(),
                    Chapter = dto.Chapter?.Trim() ?? "فصل عمومی",
                    Topic = dto.Topic.Trim(),
                    Description = dto.Description,
                    PriorityOrder = dto.PriorityOrder,
                    IsActive = true,
                    CreatedByUserId = _currentUserService.UserId,
                    CreatedAt = DateTime.Now
                };

                await _context.StudySubjectTopics.AddAsync(entity);
                await _context.SaveChangesAsync();

                dto.Id = entity.Id;
                return Ok(ApiResponse<StudySubjectTopicDto>.SuccessResult(dto, "مبحث جدید با موفقیت به بانک مباحث افزوده شد."));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<StudySubjectTopicDto>.FailureResult($"خطا در افزودن مبحث: {ex.Message}"));
            }
        }

        [HttpDelete("topics/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTopic(int id)
        {
            try
            {
                var entity = await _context.StudySubjectTopics.FindAsync(id);
                if (entity == null) return Ok(ApiResponse<bool>.FailureResult("مبحث یافت نشد."));

                entity.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();
                return Ok(ApiResponse<bool>.SuccessResult(true, "مبحث با موفقیت حذف گردید."));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<bool>.FailureResult($"خطا در حذف مبحث: {ex.Message}"));
            }
        }

        // ==========================================
        // ۹. درخواست‌های مشاوره دانش‌آموز (حضوری / آنلاین)
        // ==========================================
        [HttpGet("counseling-requests")]
        public async Task<ActionResult<ApiResponse<List<CounselingRequestDto>>>> GetCounselingRequests()
        {
            try
            {
                var query = _context.CounselingRequests.AsNoTracking().AsQueryable();

                if (_currentUserService.IsStudent)
                {
                    var sid = _currentUserService.UserId ?? 0;
                    query = query.Where(r => r.StudentUserId == sid);
                }
                else if (_currentUserService.IsConsultant && !_currentUserService.IsSystemAdmin)
                {
                    var cid = _currentUserService.UserId ?? 0;
                    query = query.Where(r => r.ConsultantUserId == cid || 
                        _context.User.Any(u => u.Id == r.StudentUserId && u.Id_Moshaver == cid));
                }

                var list = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new CounselingRequestDto
                    {
                        Id = r.Id,
                        StudentUserId = r.StudentUserId,
                        StudentName = r.StudentName,
                        ConsultantUserId = r.ConsultantUserId,
                        ConsultantName = r.ConsultantName,
                        RequestType = r.RequestType,
                        PreferredDate = r.PreferredDate,
                        PreferredTime = r.PreferredTime,
                        Description = r.Description,
                        MeetingLink = r.MeetingLink,
                        ConsultantResponse = r.ConsultantResponse,
                        ScheduledDate = r.ScheduledDate,
                        ScheduledTime = r.ScheduledTime,
                        Status = r.Status,
                        CreatedAt = r.CreatedAt
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<CounselingRequestDto>>.SuccessResult(list));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<List<CounselingRequestDto>>.FailureResult($"خطا در دریافت درخواست‌های مشاوره: {ex.Message}"));
            }
        }

        [HttpPost("counseling-requests")]
        public async Task<ActionResult<ApiResponse<CounselingRequestDto>>> CreateCounselingRequest([FromBody] CounselingRequestDto dto)
        {
            try
            {
                var currentUserId = _currentUserService.UserId ?? 0;
                var student = await _context.User.FindAsync(currentUserId);
                if (student == null) return Ok(ApiResponse<CounselingRequestDto>.FailureResult("کاربر دانش‌آموز یافت نشد."));

                var entity = new CounselingRequest
                {
                    StudentUserId = currentUserId,
                    StudentName = student.fname,
                    ConsultantUserId = student.Id_Moshaver,
                    ConsultantName = student.Moshaver,
                    RequestType = dto.RequestType,
                    PreferredDate = dto.PreferredDate,
                    PreferredTime = dto.PreferredTime,
                    Description = dto.Description,
                    MeetingLink = dto.MeetingLink,
                    Status = CounselingStatus.Pending,
                    CreatedAt = DateTime.Now
                };

                await _context.CounselingRequests.AddAsync(entity);
                await _context.SaveChangesAsync();

                dto.Id = entity.Id;
                dto.StudentName = entity.StudentName;
                dto.ConsultantName = entity.ConsultantName;
                return Ok(ApiResponse<CounselingRequestDto>.SuccessResult(dto, "درخواست وقت مشاوره شما با موفقیت ثبت شد و برای مشاور ارسال گردید."));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<CounselingRequestDto>.FailureResult($"خطا در ثبت درخواست مشاوره: {ex.Message}"));
            }
        }

        [HttpPut("counseling-requests/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateCounselingRequest(int id, [FromBody] CounselingRequestDto dto)
        {
            try
            {
                var entity = await _context.CounselingRequests.FindAsync(id);
                if (entity == null) return Ok(ApiResponse<bool>.FailureResult("درخواست یافت نشد."));

                entity.Status = dto.Status;
                entity.ConsultantResponse = dto.ConsultantResponse;
                entity.ScheduledDate = dto.ScheduledDate ?? dto.PreferredDate;
                entity.ScheduledTime = dto.ScheduledTime ?? dto.PreferredTime;
                entity.MeetingLink = dto.MeetingLink ?? entity.MeetingLink;
                entity.UpdatedAt = DateTime.Now;

                // اگر تایید شد، یک برنامه ریزی خودکار در تقویم برای آن روز ثبت شود!
                if (dto.Status == CounselingStatus.Approved && !string.IsNullOrWhiteSpace(entity.ScheduledDate))
                {
                    DateTime gDate = DateTime.Now;
                    if (PersianDateHelper.TryParsePersianDate(entity.ScheduledDate, out var parsed))
                        gDate = parsed;

                    var planItem = new StudyPlanItem
                    {
                        ConsultantUserId = _currentUserService.UserId ?? entity.ConsultantUserId ?? 0,
                        ConsultantName = _currentUserService.FullName ?? entity.ConsultantName ?? "مشاور",
                        StudentUserId = entity.StudentUserId,
                        StudentName = entity.StudentName,
                        PersianDate = entity.ScheduledDate,
                        GregorianDate = gDate,
                        StartTime = entity.ScheduledTime ?? "10:00",
                        DurationMinutes = 45,
                        ActivityType = entity.RequestType == "مشاوره حضوری" ? ActivityTypes.InPersonCounseling : ActivityTypes.OnlineCounseling,
                        Subject = "جلسه مشاوره و راهنمایی تحصیلی",
                        Topic = entity.Description,
                        ConsultantNote = entity.ConsultantResponse,
                        IsCounselingSession = true,
                        CounselingType = entity.RequestType,
                        MeetingLink = entity.MeetingLink,
                        Status = StudyTaskStatus.Planned,
                        CreatedAt = DateTime.Now
                    };

                    await _context.StudyPlanItems.AddAsync(planItem);
                    await _context.SaveChangesAsync();
                    entity.CreatedStudyPlanId = planItem.Id;
                }

                await _context.SaveChangesAsync();
                return Ok(ApiResponse<bool>.SuccessResult(true, "وضعیت درخواست مشاوره به‌روزرسانی شد."));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<bool>.FailureResult($"خطا در به‌روزرسانی درخواست: {ex.Message}"));
            }
        }
    }
}
