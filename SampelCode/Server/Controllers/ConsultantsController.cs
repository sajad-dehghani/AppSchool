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
using NovinApp.Shared.DTOs.Consultants;
using NovinApp.Shared.Entities;

namespace NovinApp.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultantsController : ControllerBase
    {
        private readonly MyAppContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;

        public ConsultantsController(
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
        public async Task<ActionResult<ApiResponse<List<ConsultantDto>>>> GetAll()
        {
            var query = _context.ConsultantProfiles
                .AsNoTracking();

            // اگر خود مشاور درخواست داد و ادمین نبود، فقط پروفایل خودش را دریافت کند
            if (_currentUserService.IsConsultant && !_currentUserService.IsSystemAdmin)
            {
                query = query.Where(c => c.UserId == _currentUserService.UserId);
            }

            var list = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var userIds = list.Select(c => c.UserId).Distinct().ToList();
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var dtos = list.Select(c =>
            {
                users.TryGetValue(c.UserId, out var u);
                return new ConsultantDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    FullName = u?.FullName ?? "نامشخص",
                    NationalCode = u?.NationalCode ?? "",
                    PhoneNumber = u?.PhoneNumber,
                    Specialty = c.Specialty,
                    Bio = c.Bio,
                    ProfilePictureUrl = u?.ProfilePictureUrl,
                    StudentsCount = _context.StudentProfiles.Count(s => s.ConsultantId == c.Id),
                    IsActive = c.IsActive && (u?.IsActive ?? false),
                    CreatedAt = c.CreatedAt
                };
            }).ToList();

            return Ok(ApiResponse<List<ConsultantDto>>.Ok(dtos));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ConsultantDto>>> GetById(int id)
        {
            var c = await _context.ConsultantProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (c == null)
                return NotFound(ApiResponse<ConsultantDto>.Fail("مشاور مورد نظر یافت نشد."));

            // IDOR Protection: مشاور عادی فقط به پروفایل خودش دسترسی دارد
            if (_currentUserService.IsConsultant && !_currentUserService.IsSystemAdmin && c.UserId != _currentUserService.UserId)
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(c.UserId.ToString());

            var dto = new ConsultantDto
            {
                Id = c.Id,
                UserId = c.UserId,
                FullName = user?.FullName ?? "",
                NationalCode = user?.NationalCode ?? "",
                PhoneNumber = user?.PhoneNumber,
                Specialty = c.Specialty,
                Bio = c.Bio,
                ProfilePictureUrl = user?.ProfilePictureUrl,
                StudentsCount = await _context.StudentProfiles.CountAsync(s => s.ConsultantId == c.Id),
                IsActive = c.IsActive && (user?.IsActive ?? false),
                CreatedAt = c.CreatedAt
            };

            return Ok(ApiResponse<ConsultantDto>.Ok(dto));
        }

        [Authorize(Roles = UserRoles.SystemAdmin)]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ConsultantDto>>> Create([FromBody] CreateConsultantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ConsultantDto>.Fail("اطلاعات ارسالی نامعتبر است."));

            var trimmedNationalCode = dto.NationalCode.Trim();

            // بررسی تکراری نبودن کد ملی
            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.NationalCode == trimmedNationalCode || u.UserName == trimmedNationalCode);

            if (existingUser != null)
                return BadRequest(ApiResponse<ConsultantDto>.Fail("کاربری با این کد ملی از قبل در سامانه ثبت شده است."));

            // ایجاد کاربر Identity
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
                return BadRequest(ApiResponse<ConsultantDto>.Fail("خطا در ایجاد حساب کاربری مشاور.", errors));
            }

            await _userManager.AddToRoleAsync(user, UserRoles.Consultant);

            // ایجاد پروفایل مشاور
            var profile = new ConsultantProfile
            {
                UserId = user.Id,
                Specialty = dto.Specialty?.Trim(),
                Bio = dto.Bio?.Trim(),
                IsActive = true
            };

            _context.ConsultantProfiles.Add(profile);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("CreateConsultant", "ConsultantProfile", profile.Id.ToString(), $"ثبت مشاور جدید: {user.FullName}");

            var resultDto = new ConsultantDto
            {
                Id = profile.Id,
                UserId = user.Id,
                FullName = user.FullName,
                NationalCode = user.NationalCode,
                PhoneNumber = user.PhoneNumber,
                Specialty = profile.Specialty,
                Bio = profile.Bio,
                IsActive = true,
                CreatedAt = profile.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = profile.Id }, ApiResponse<ConsultantDto>.Ok(resultDto, "مشاور با موفقیت ثبت شد."));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Update(int id, [FromBody] UpdateConsultantDto dto)
        {
            if (id != dto.Id || !ModelState.IsValid)
                return BadRequest(ApiResponse<bool>.Fail("اطلاعات ارسالی نامعتبر است."));

            var profile = await _context.ConsultantProfiles.FindAsync(id);
            if (profile == null)
                return NotFound(ApiResponse<bool>.Fail("مشاور یافت نشد."));

            // IDOR Protection: فقط ادمین یا خود مشاور مجاز به ویرایش است
            if (!_currentUserService.IsSystemAdmin && profile.UserId != _currentUserService.UserId)
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(profile.UserId.ToString());
            if (user == null)
                return NotFound(ApiResponse<bool>.Fail("حساب کاربری مشاور یافت نشد."));

            user.FullName = dto.FullName.Trim();
            user.PhoneNumber = dto.PhoneNumber?.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Gender))
                user.Gender = dto.Gender;

            if (_currentUserService.IsSystemAdmin)
            {
                user.IsActive = dto.IsActive;
                profile.IsActive = dto.IsActive;
            }

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            }

            await _userManager.UpdateAsync(user);

            profile.Specialty = dto.Specialty?.Trim();
            profile.Bio = dto.Bio?.Trim();
            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("UpdateConsultant", "ConsultantProfile", profile.Id.ToString(), $"ویرایش اطلاعات مشاور: {user.FullName}");

            return Ok(ApiResponse<bool>.Ok(true, "اطلاعات مشاور با موفقیت به‌روزرسانی شد."));
        }

        [Authorize(Roles = UserRoles.SystemAdmin)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var profile = await _context.ConsultantProfiles.FindAsync(id);
            if (profile == null)
                return NotFound(ApiResponse<bool>.Fail("مشاور یافت نشد."));

            var hasStudents = await _context.StudentProfiles.AnyAsync(s => s.ConsultantId == id);
            if (hasStudents)
            {
                return BadRequest(ApiResponse<bool>.Fail("این مشاور دارای دانش‌آموزان تحت نظر است. لطفاً ابتدا دانش‌آموزان را به مشاور دیگری منتسب فرمایید."));
            }

            var user = await _userManager.FindByIdAsync(profile.UserId.ToString());

            _context.ConsultantProfiles.Remove(profile);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("DeleteConsultant", "ConsultantProfile", id.ToString(), $"حذف مشاور {user?.FullName}");

            return Ok(ApiResponse<bool>.Ok(true, "مشاور با موفقیت حذف شد."));
        }
    }
}
