using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovinApp.Server.MyContext;
using NovinApp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovinApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PsychologyTestController : ControllerBase
    {
        private readonly MyAppContext _context;

        public PsychologyTestController(MyAppContext context)
        {
            _context = context;
        }

        // بررسی اینکه آیا دانش‌آموز قبلاً در این آزمون شرکت کرده یا خیر
        [HttpGet("check-status/{userId}/{testType}")]
        public async Task<IActionResult> CheckStatus(int userId, string testType)
        {
            var existing = await _context.PsychologyTestResults
                .Where(r => r.UserId == userId && r.TestType.ToLower() == testType.ToLower())
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                return Ok(new { hasTaken = true, result = existing });
            }

            return Ok(new { hasTaken = false, result = (PsychologyTestResult?)null });
        }

        // ذخیره کارنامه جدید با تضمین عدم شرکت مجدد
        [HttpPost("save")]
        public async Task<IActionResult> SaveResult([FromBody] PsychologyTestResult model)
        {
            if (model == null)
                return BadRequest("اطلاعات ارسال شده نامعتبر است.");

            // بررسی محدودیت تک‌بار آزمون
            var alreadyTaken = await _context.PsychologyTestResults
                .AnyAsync(r => r.UserId == model.UserId && r.TestType.ToLower() == model.TestType.ToLower());

            if (alreadyTaken)
            {
                return BadRequest("شما قبلاً در این آزمون شرکت کرده‌اید. هر دانش‌آموز تنها یک بار مجاز به شرکت در هر آزمون است.");
            }

            // خواندن مشخصات مشاور دانش‌آموز در صورت خالی بودن
            if (model.Id_Moshaver == null || model.Id_Moshaver <= 0)
            {
                var user = await _context.User.FindAsync(model.UserId);
                if (user != null)
                {
                    model.Id_Moshaver = user.Id_Moshaver;
                    if (string.IsNullOrWhiteSpace(model.StudentName))
                        model.StudentName = user.fname;
                    if (string.IsNullOrWhiteSpace(model.StudentNationalCode))
                        model.StudentNationalCode = user.code_meli;
                }
            }

            model.CreatedAt = DateTime.Now;
            _context.PsychologyTestResults.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // دریافت تمام کارنامه‌های یک دانش‌آموز
        [HttpGet("my-results/{userId}")]
        public async Task<IActionResult> GetMyResults(int userId)
        {
            var results = await _context.PsychologyTestResults
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(results);
        }

        // دسترسی مشاور به کارنامه‌های دانش‌آموزان تحت نظر خود (یا تمام کارنامه‌ها در صورت مدیر بودن)
        [HttpGet("counselor-students/{counselorId}")]
        public async Task<IActionResult> GetCounselorStudentsTests(int counselorId)
        {
            var query = _context.PsychologyTestResults.AsQueryable();
            if (counselorId > 0)
            {
                query = query.Where(r => r.Id_Moshaver == counselorId);
            }

            var results = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(results);
        }

        // مشاهده کارنامه مشخص بر اساس Id
        [HttpGet("result/{id}")]
        public async Task<IActionResult> GetResultById(int id)
        {
            var result = await _context.PsychologyTestResults.FindAsync(id);
            if (result == null)
                return NotFound("کارنامه مورد نظر یافت نشد.");

            return Ok(result);
        }

        // ذخیره یا به‌روزرسانی نظریه نهایی هوش مصنوعی
        [HttpPost("update-ai-guidance/{id}")]
        public async Task<IActionResult> UpdateAiGuidance(int id, [FromBody] string aiGuidance)
        {
            var result = await _context.PsychologyTestResults.FindAsync(id);
            if (result == null)
                return NotFound("کارنامه یافت نشد.");

            result.AiRecommendation = aiGuidance;
            result.IsAiGenerated = true;
            await _context.SaveChangesAsync();

            return Ok(result);
        }
    }
}
