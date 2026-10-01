using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovinApp.Server.Models;
using NovinApp.Server.MyContext;
using NovinApp.Server.Services;
using NovinApp.Shared.Constants;
using NovinApp.Shared.DTOs.Common;
using NovinApp.Shared.DTOs.Students;
using NovinApp.Shared.Entities;

namespace NovinApp.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly MyAppContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;

        public StudentsController(
            MyAppContext context,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<StudentDto>>>> GetAll(
            [FromQuery] int? schoolId = null,
            [FromQuery] int? consultantId = null,
            [FromQuery] string? search = null)
        {
            var query = _context.StudentProfiles
                .AsNoTracking()
                .Include(s => s.School)
                .Include(s => s.Consultant)
                .AsQueryable();

            // ===== داده‌کاوی و محدودسازی امنیتی بر اساس نقش (Data Scoping) =====
            if (_currentUserService.IsStudent)
            {
                // دانش‌آموز فقط اطلاعات خودش را می‌بیند
                query = query.Where(s => s.UserId == _currentUserService.UserId);
            }
            else if (_currentUserService.IsConsultant && !_currentUserService.IsSystemAdmin)
            {
                // مشاور فقط دانش‌آموزان تحت نظر خودش را می‌بیند
                query = query.Where(s => s.ConsultantId == _currentUserService.ConsultantId);
            }
            else if (_currentUserService.IsSchoolManager && !_currentUserService.IsSystemAdmin)
            {
                // مدیر مدرسه فقط دانش‌آموزان مدرسه خودش را می‌بیند
                query = query.Where(s => s.SchoolId == _currentUserService.SchoolId);
            }
            else if (_currentUserService.IsSystemAdmin)
            {
                // ادمین امکان فیلتر بر اساس مدرسه و مشاور را دارد
                if (schoolId.HasValue)
                    query = query.Where(s => s.SchoolId == schoolId.Value);

                if (consultantId.HasValue)
                    query = query.Where(s => s.ConsultantId == consultantId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(x => x.NationalCode.Contains(s) || (x.StudentCode != null && x.StudentCode.ToString()!.Contains(s)));
            }

            var list = await query
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var userIds = list.Select(s => s.UserId).Distinct().ToList();
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            // مشاوران جهت استخراج نام
            var consultantProfileIds = list.Where(s => s.ConsultantId.HasValue).Select(s => s.ConsultantId!.Value).Distinct().ToList();
            var consultantProfiles = await _context.ConsultantProfiles
                .Where(c => consultantProfileIds.Contains(c.Id))
                .ToListAsync();
            var consultantUserIds = consultantProfiles.Select(c => c.UserId).ToList();
            var consultantUsers = await _userManager.Users
                .Where(u => consultantUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var dtos = list.Select(s =>
            {
                users.TryGetValue(s.UserId, out var u);
                string? consName = null;
                if (s.ConsultantId.HasValue)
                {
                    var cp = consultantProfiles.FirstOrDefault(c => c.Id == s.ConsultantId.Value);
                    if (cp != null && consultantUsers.TryGetValue(cp.UserId, out var cu))
                    {
                        consName = cu.FullName;
                    }
                }

                return new StudentDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    FullName = u?.FullName ?? "نامشخص",
                    NationalCode = s.NationalCode,
                    StudentCode = s.StudentCode,
                    PhoneNumber = u?.PhoneNumber,
                    FatherName = s.FatherName,
                    GradeLevel = s.GradeLevel,
                    FieldOfStudy = s.FieldOfStudy,
                    SchoolId = s.SchoolId,
                    SchoolName = s.School?.Name,
                    ConsultantId = s.ConsultantId,
                    ConsultantName = consName,
                    ProfilePictureUrl = u?.ProfilePictureUrl,
                    IsActive = s.IsActive && (u?.IsActive ?? false),
                    CreatedAt = s.CreatedAt
                };
            }).ToList();

            return Ok(ApiResponse<List<StudentDto>>.Ok(dtos));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<StudentDto>>> GetById(int id)
        {
            var s = await _context.StudentProfiles
                .AsNoTracking()
                .Include(x => x.School)
                .Include(x => x.Consultant)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (s == null)
                return NotFound(ApiResponse<StudentDto>.Fail("دانش‌آموز مورد نظر یافت نشد."));

            // ===== IDOR Protection =====
            if (_currentUserService.IsStudent && s.UserId != _currentUserService.UserId)
                return Forbid();

            if (_currentUserService.IsConsultant && !_currentUserService.IsSystemAdmin && s.ConsultantId != _currentUserService.ConsultantId)
                return Forbid();

            if (_currentUserService.IsSchoolManager && !_currentUserService.IsSystemAdmin && s.SchoolId != _currentUserService.SchoolId)
                return Forbid();

            var user = await _userManager.FindByIdAsync(s.UserId.ToString());

            string? consultantName = null;
            if (s.Consultant != null)
            {
                var cu = await _userManager.FindByIdAsync(s.Consultant.UserId.ToString());
                consultantName = cu?.FullName;
            }

            var dto = new StudentDto
            {
                Id = s.Id,
                UserId = s.UserId,
                FullName = user?.FullName ?? "",
                NationalCode = s.NationalCode,
                StudentCode = s.StudentCode,
                PhoneNumber = user?.PhoneNumber,
                FatherName = s.FatherName,
                GradeLevel = s.GradeLevel,
                FieldOfStudy = s.FieldOfStudy,
                SchoolId = s.SchoolId,
                SchoolName = s.School?.Name,
                ConsultantId = s.ConsultantId,
                ConsultantName = consultantName,
                ProfilePictureUrl = user?.ProfilePictureUrl,
                IsActive = s.IsActive && (user?.IsActive ?? false),
                CreatedAt = s.CreatedAt
            };

            return Ok(ApiResponse<StudentDto>.Ok(dto));
        }

        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.SchoolManager}")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<StudentDto>>> Create([FromBody] CreateStudentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<StudentDto>.Fail("اطلاعات ارسالی نامعتبر است."));

            var trimmedNationalCode = dto.NationalCode.Trim();

            // بررسی عدم وجود کاربر با این کد ملی
            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.NationalCode == trimmedNationalCode || u.UserName == trimmedNationalCode);

            if (existingUser != null)
                return BadRequest(ApiResponse<StudentDto>.Fail("کاربری با این کد ملی قبلاً در سامانه ثبت شده است."));

            // اگر مدیر مدرسه در حال ایجاد دانش‌آموز است، حتماً مدرسه مربوط به خود او انتساب یابد
            int? targetSchoolId = dto.SchoolId;
            if (_currentUserService.IsSchoolManager && !_currentUserService.IsSystemAdmin)
            {
                targetSchoolId = _currentUserService.SchoolId;
            }

            var user = new ApplicationUser
            {
                UserName = trimmedNationalCode,
                NationalCode = trimmedNationalCode,
                FullName = dto.FullName.Trim(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                Gender = dto.Gender ?? "مرد",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                return BadRequest(ApiResponse<StudentDto>.Fail("خطا در ایجاد کاربر دانش‌آموز.", errors));
            }

            await _userManager.AddToRoleAsync(user, UserRoles.Student);

            var profile = new StudentProfile
            {
                UserId = user.Id,
                NationalCode = trimmedNationalCode,
                StudentCode = dto.StudentCode,
                FatherName = dto.FatherName?.Trim(),
                GradeLevel = dto.GradeLevel?.Trim(),
                FieldOfStudy = dto.FieldOfStudy?.Trim(),
                SchoolId = targetSchoolId,
                ConsultantId = dto.ConsultantId,
                IsActive = true
            };

            _context.StudentProfiles.Add(profile);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("CreateStudent", "StudentProfile", profile.Id.ToString(), $"ثبت دانش‌آموز: {user.FullName} ({trimmedNationalCode})");

            var resultDto = new StudentDto
            {
                Id = profile.Id,
                UserId = user.Id,
                FullName = user.FullName,
                NationalCode = user.NationalCode,
                StudentCode = profile.StudentCode,
                PhoneNumber = user.PhoneNumber,
                FatherName = profile.FatherName,
                GradeLevel = profile.GradeLevel,
                FieldOfStudy = profile.FieldOfStudy,
                SchoolId = profile.SchoolId,
                ConsultantId = profile.ConsultantId,
                IsActive = true,
                CreatedAt = profile.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = profile.Id }, ApiResponse<StudentDto>.Ok(resultDto, "دانش‌آموز با موفقیت ثبت شد."));
        }

        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.SchoolManager},{UserRoles.Consultant}")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Update(int id, [FromBody] UpdateStudentDto dto)
        {
            if (id != dto.Id || !ModelState.IsValid)
                return BadRequest(ApiResponse<bool>.Fail("اطلاعات ارسالی نامعتبر است."));

            var profile = await _context.StudentProfiles.FindAsync(id);
            if (profile == null)
                return NotFound(ApiResponse<bool>.Fail("دانش‌آموز یافت نشد."));

            // IDOR Protection
            if (_currentUserService.IsSchoolManager && !_currentUserService.IsSystemAdmin && profile.SchoolId != _currentUserService.SchoolId)
                return Forbid();

            if (_currentUserService.IsConsultant && !_currentUserService.IsSystemAdmin && profile.ConsultantId != _currentUserService.ConsultantId)
                return Forbid();

            var user = await _userManager.FindByIdAsync(profile.UserId.ToString());
            if (user == null)
                return NotFound(ApiResponse<bool>.Fail("کاربر دانش‌آموز یافت نشد."));

            user.FullName = dto.FullName.Trim();
            user.PhoneNumber = dto.PhoneNumber?.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Gender))
                user.Gender = dto.Gender;

            if (_currentUserService.IsSystemAdmin)
            {
                user.IsActive = dto.IsActive;
                profile.IsActive = dto.IsActive;
                profile.SchoolId = dto.SchoolId;
                profile.ConsultantId = dto.ConsultantId;
            }
            else if (_currentUserService.IsSchoolManager)
            {
                profile.ConsultantId = dto.ConsultantId; // مدیر می‌تواند مشاور را تخصیص دهد
            }

            profile.StudentCode = dto.StudentCode;
            profile.FatherName = dto.FatherName?.Trim();
            profile.GradeLevel = dto.GradeLevel?.Trim();
            profile.FieldOfStudy = dto.FieldOfStudy?.Trim();
            profile.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.NewPassword) && _currentUserService.IsSystemAdmin)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            }

            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("UpdateStudent", "StudentProfile", profile.Id.ToString(), $"ویرایش دانش‌آموز: {user.FullName}");

            return Ok(ApiResponse<bool>.Ok(true, "اطلاعات دانش‌آموز با موفقیت به‌روزرسانی شد."));
        }

        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.SchoolManager}")]
        [HttpPut("{id}/assign")]
        public async Task<ActionResult<ApiResponse<bool>>> Assign(int id, [FromBody] AssignStudentDto dto)
        {
            var student = await _context.StudentProfiles.FindAsync(id);
            if (student == null)
                return NotFound(ApiResponse<bool>.Fail("دانش‌آموز یافت نشد."));

            // اگر مدیر مدرسه است، فقط می‌تواند دانش‌آموز مدرسه خود را مدیریت کند
            if (_currentUserService.IsSchoolManager && !_currentUserService.IsSystemAdmin && student.SchoolId != _currentUserService.SchoolId)
                return Forbid();

            if (_currentUserService.IsSystemAdmin && dto.SchoolId.HasValue)
            {
                student.SchoolId = dto.SchoolId.Value;
            }

            if (dto.ConsultantId.HasValue)
            {
                student.ConsultantId = dto.ConsultantId.Value;
            }

            student.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("AssignStudent", "StudentProfile", id.ToString(), $"انتساب دانش‌آموز به مدرسه {dto.SchoolId} و مشاور {dto.ConsultantId}");

            return Ok(ApiResponse<bool>.Ok(true, "انتساب با موفقیت انجام شد."));
        }

        [Authorize(Roles = UserRoles.SystemAdmin)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var profile = await _context.StudentProfiles
                .Include(s => s.ExamResults)
                .Include(s => s.StudyPlans)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (profile == null)
                return NotFound(ApiResponse<bool>.Fail("دانش‌آموز یافت نشد."));

            var user = await _userManager.FindByIdAsync(profile.UserId.ToString());

            _context.StudentProfiles.Remove(profile);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("DeleteStudent", "StudentProfile", id.ToString(), $"حذف دانش‌آموز {user?.FullName}");

            return Ok(ApiResponse<bool>.Ok(true, "دانش‌آموز با موفقیت حذف شد."));
        }
    }
}
