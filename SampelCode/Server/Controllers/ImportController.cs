using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovinApp.Server.Services;
using NovinApp.Shared.Constants;
using NovinApp.Shared.DTOs.Common;
using NovinApp.Shared.DTOs.Import;
using NovinApp.Shared.Enums;

namespace NovinApp.Server.Controllers
{
    [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.SchoolManager},{UserRoles.Consultant}")]
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IExcelImportService _importService;
        private readonly ICurrentUserService _currentUser;

        public ImportController(IExcelImportService importService, ICurrentUserService currentUser)
        {
            _importService = importService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// ایمپورت لیست دانش‌آموزان از فایل اکسل
        /// </summary>
        [HttpPost("students")]
        [RequestSizeLimit(10 * 1024 * 1024)] // حداکثر ۱۰ مگابایت
        public async Task<ActionResult<ApiResponse<ImportResultDto>>> ImportStudents(
            IFormFile file,
            [FromQuery] int? schoolId,
            [FromQuery] int? consultantId,
            [FromQuery] DuplicateHandlingMode duplicateHandling = DuplicateHandlingMode.Skip)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<ImportResultDto>.Fail("لطفاً یک فایل اکسل انتخاب نمایید."));

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".xlsx" && ext != ".xls")
                return BadRequest(ApiResponse<ImportResultDto>.Fail("فقط فایل‌های اکسل (xlsx, xls) قابل قبول هستند."));

            using var stream = file.OpenReadStream();
            var result = await _importService.ImportStudentsAsync(
                stream, file.FileName, _currentUser.UserId ?? 0, schoolId, consultantId, duplicateHandling);

            return Ok(ApiResponse<ImportResultDto>.Ok(result,
                result.Status == ImportStatus.Failed ? (result.SummaryMessage ?? "خطا در پردازش فایل") : "ایمپورت با موفقیت انجام شد."));
        }

        /// <summary>
        /// ایمپورت نتایج آزمون دانش‌آموزان از فایل اکسل
        /// </summary>
        [HttpPost("exam-results")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<ActionResult<ApiResponse<ImportResultDto>>> ImportExamResults(
            IFormFile file,
            [FromQuery] int examId,
            [FromQuery] DuplicateHandlingMode duplicateHandling = DuplicateHandlingMode.Skip)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<ImportResultDto>.Fail("لطفاً یک فایل اکسل انتخاب نمایید."));

            if (examId <= 0)
                return BadRequest(ApiResponse<ImportResultDto>.Fail("آزمون مورد نظر را انتخاب نمایید."));

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".xlsx" && ext != ".xls")
                return BadRequest(ApiResponse<ImportResultDto>.Fail("فقط فایل‌های اکسل قابل قبول هستند."));

            using var stream = file.OpenReadStream();
            var result = await _importService.ImportExamResultsAsync(
                stream, file.FileName, _currentUser.UserId ?? 0, examId, duplicateHandling);

            return Ok(ApiResponse<ImportResultDto>.Ok(result,
                result.Status == ImportStatus.Failed ? (result.SummaryMessage ?? "خطا در پردازش فایل نتایج") : "ایمپورت نتایج آزمون انجام شد."));
        }

        /// <summary>
        /// تاریخچه ایمپورت‌ها
        /// </summary>
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<ImportSessionListDto>>>> GetHistory()
        {
            int? userId = _currentUser.IsSystemAdmin ? null : _currentUser.UserId;
            var result = await _importService.GetImportHistoryAsync(userId);
            return Ok(ApiResponse<List<ImportSessionListDto>>.Ok(result));
        }

        /// <summary>
        /// جزئیات یک ایمپورت خاص
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<ImportResultDto>>> GetDetail(int id)
        {
            var result = await _importService.GetImportDetailAsync(id);
            if (result == null)
                return NotFound(ApiResponse<ImportResultDto>.Fail("سشن ایمپورت یافت نشد."));

            return Ok(ApiResponse<ImportResultDto>.Ok(result));
        }
    }
}
