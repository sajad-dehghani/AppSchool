using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovinApp.Server.Services;
using NovinApp.Shared.Constants;
using NovinApp.Shared.DTOs.Common;
using NovinApp.Shared.DTOs.Exams;

namespace NovinApp.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ExamsController : ControllerBase
    {
        private readonly IExamReportService _examService;
        private readonly ICurrentUserService _currentUser;

        public ExamsController(IExamReportService examService, ICurrentUserService currentUser)
        {
            _examService = examService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// دریافت لیست تمام آزمون‌ها
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ExamDto>>>> GetAll([FromQuery] string? search = null)
        {
            var exams = await _examService.GetExamsAsync(search);
            return Ok(ApiResponse<List<ExamDto>>.Ok(exams));
        }

        /// <summary>
        /// روت سازگار برای کلاینت‌های قدیمی
        /// </summary>
        [HttpGet("all")]
        public async Task<ActionResult<ApiResponse<List<ExamDto>>>> GetAllLegacy()
        {
            var exams = await _examService.GetExamsAsync();
            return Ok(ApiResponse<List<ExamDto>>.Ok(exams));
        }

        /// <summary>
        /// دریافت مشخصات یک آزمون
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<ExamDto>>> GetById(int id)
        {
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
                return NotFound(ApiResponse<ExamDto>.Fail("آزمون مورد نظر یافت نشد."));

            return Ok(ApiResponse<ExamDto>.Ok(exam));
        }

        /// <summary>
        /// ایجاد آزمون جدید
        /// </summary>
        [HttpPost]
        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.SchoolManager},{UserRoles.Consultant}")]
        public async Task<ActionResult<ApiResponse<ExamDto>>> Create([FromBody] CreateExamDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ExamDto>.Fail("اطلاعات ورودی نامعتبر است."));

            var created = await _examService.CreateExamAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<ExamDto>.Ok(created, "آزمون با موفقیت ایجاد شد."));
        }

        /// <summary>
        /// ویرایش مشخصات آزمون
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.SchoolManager},{UserRoles.Consultant}")]
        public async Task<ActionResult<ApiResponse<ExamDto>>> Update(int id, [FromBody] UpdateExamDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ExamDto>.Fail("اطلاعات ورودی نامعتبر است."));

            var updated = await _examService.UpdateExamAsync(id, dto);
            if (updated == null)
                return NotFound(ApiResponse<ExamDto>.Fail("آزمون مورد نظر یافت نشد."));

            return Ok(ApiResponse<ExamDto>.Ok(updated, "مشخصات آزمون به‌روزرسانی شد."));
        }

        /// <summary>
        /// حذف آزمون و نتایج مربوطه
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.SystemAdmin)]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var success = await _examService.DeleteExamAsync(id);
            if (!success)
                return NotFound(ApiResponse<bool>.Fail("آزمون مورد نظر یافت نشد."));

            return Ok(ApiResponse<bool>.Ok(true, "آزمون با موفقیت حذف گردید."));
        }

        /// <summary>
        /// دریافت لیست نتایج دانش‌آموزان در یک آزمون
        /// </summary>
        [HttpGet("{id:int}/results")]
        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.SchoolManager},{UserRoles.Consultant}")]
        public async Task<ActionResult<ApiResponse<List<ExamStudentResultItemDto>>>> GetResults(
            int id,
            [FromQuery] int? schoolId = null,
            [FromQuery] int? consultantId = null,
            [FromQuery] string? search = null)
        {
            // اعمال محدودیت دسترسی داده‌ها (Scoping)
            int? effectiveSchoolId = schoolId;
            int? effectiveConsultantId = consultantId;

            if (_currentUser.IsSchoolManager && !_currentUser.IsSystemAdmin)
            {
                effectiveSchoolId = _currentUser.SchoolId;
            }
            else if (_currentUser.IsConsultant && !_currentUser.IsSystemAdmin)
            {
                effectiveConsultantId = _currentUser.ConsultantId;
            }

            var results = await _examService.GetExamResultsAsync(id, effectiveSchoolId, effectiveConsultantId, search);
            return Ok(ApiResponse<List<ExamStudentResultItemDto>>.Ok(results));
        }

        /// <summary>
        /// کارنامه تحلیلی کامل یک دانش‌آموز در آزمون
        /// </summary>
        [HttpGet("{examId:int}/report-card/{studentProfileId:int}")]
        public async Task<ActionResult<ApiResponse<ReportCardDto>>> GetReportCard(int examId, int studentProfileId)
        {
            // بررسی امنیتی دسترسی به کارنامه
            if (_currentUser.IsStudent && _currentUser.StudentId != studentProfileId)
            {
                return Forbid();
            }

            var reportCard = await _examService.GetStudentReportCardAsync(examId, studentProfileId);
            if (reportCard == null)
                return NotFound(ApiResponse<ReportCardDto>.Fail("کارنامه‌ای برای این داوطلب در این آزمون ثبت نشده است."));

            return Ok(ApiResponse<ReportCardDto>.Ok(reportCard));
        }

        /// <summary>
        /// دریافت کارنامه با کد ملی
        /// </summary>
        [HttpGet("{examId:int}/report-card/by-nationalcode/{nationalCode}")]
        public async Task<ActionResult<ApiResponse<ReportCardDto>>> GetReportCardByNationalCode(int examId, string nationalCode)
        {
            if (_currentUser.IsStudent && _currentUser.NationalCode != nationalCode)
            {
                return Forbid();
            }

            var reportCard = await _examService.GetStudentReportCardByNationalCodeAsync(examId, nationalCode);
            if (reportCard == null)
                return NotFound(ApiResponse<ReportCardDto>.Fail("کارنامه‌ای برای این داوطلب در این آزمون ثبت نشده است."));

            return Ok(ApiResponse<ReportCardDto>.Ok(reportCard));
        }

        /// <summary>
        /// روند پیشرفت تحصیلی دانش‌آموز در آزمون‌ها
        /// </summary>
        [HttpGet("student/{studentProfileId:int}/history")]
        public async Task<ActionResult<ApiResponse<List<StudentExamHistoryItemDto>>>> GetStudentHistory(int studentProfileId)
        {
            if (_currentUser.IsStudent && _currentUser.StudentId != studentProfileId)
            {
                return Forbid();
            }

            var history = await _examService.GetStudentExamHistoryAsync(studentProfileId);
            return Ok(ApiResponse<List<StudentExamHistoryItemDto>>.Ok(history));
        }

        /// <summary>
        /// آمار و تحلیل عملکرد در آزمون
        /// </summary>
        [HttpGet("{id:int}/analytics")]
        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.SchoolManager},{UserRoles.Consultant}")]
        public async Task<ActionResult<ApiResponse<ExamAnalyticsDto>>> GetAnalytics(int id)
        {
            var analytics = await _examService.GetExamAnalyticsAsync(id);
            if (analytics == null)
                return NotFound(ApiResponse<ExamAnalyticsDto>.Fail("آزمون مورد نظر یافت نشد."));

            return Ok(ApiResponse<ExamAnalyticsDto>.Ok(analytics));
        }

        /// <summary>
        /// محاسبه مجدد رتبه‌ها، میانگین‌ها و وضعیت دروس
        /// </summary>
        [HttpPost("{id:int}/calculate-ranks")]
        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.SchoolManager}")]
        public async Task<ActionResult<ApiResponse<bool>>> CalculateRanks(int id)
        {
            var success = await _examService.CalculateRanksAndAveragesAsync(id);
            if (!success)
                return BadRequest(ApiResponse<bool>.Fail("نتایجی برای محاسبه در این آزمون یافت نشد."));

            return Ok(ApiResponse<bool>.Ok(true, "محاسبه رتبه‌ها و میانگین‌ها با موفقیت انجام شد."));
        }
    }
}
