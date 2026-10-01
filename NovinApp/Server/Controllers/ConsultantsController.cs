using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovinApp.Server.Helpers;
using NovinApp.Server.MyContext;
using NovinApp.Server.Services;
using NovinApp.Shared;
using NovinApp.Shared.Constants;
using NovinApp.Shared.DTOs.Common;
using NovinApp.Shared.DTOs.Consultants;

namespace NovinApp.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultantsController : ControllerBase
    {
        private readonly MyAppContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;

        public ConsultantsController(MyAppContext context, ICurrentUserService currentUserService, IAuditService auditService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ConsultantDto>>>> GetAll()
        {
            try
            {
                var consultantUsers = await _context.User
                    .AsNoTracking()
                    .Where(u => u.Rool == UserRoles.Consultant ||
                               u.Rool == "مشاور" ||
                               u.Rool == "مشاور تحصیلی" ||
                               u.Rool == "Moshaver")
                    .OrderBy(u => u.fname)
                    .ToListAsync();

                var userIds = consultantUsers.Select(u => u.Id).ToList();

                var profiles = await _context.ConsultantProfiles
                    .AsNoTracking()
                    .Where(p => userIds.Contains(p.UserId))
                    .ToListAsync();

                var dtos = consultantUsers.Select(u =>
                {
                    var profile = profiles.FirstOrDefault(p => p.UserId == u.Id);
                    var studentsCount = _context.User.Count(s =>
                        (s.Rool == UserRoles.Student || s.Rool == "دانش آموز" || s.Rool == "دانش_آموز" || s.Rool == "Student") &&
                        s.Id_Moshaver == u.Id);

                    return new ConsultantDto
                    {
                        Id = profile?.Id ?? u.Id,
                        UserId = u.Id,
                        FullName = u.fname ?? "",
                        NationalCode = u.code_meli ?? "",
                        PhoneNumber = u.mobile,
                        Specialty = profile?.Specialty,
                        Bio = profile?.Bio,
                        ProfilePictureUrl = u.pic,
                        StudentsCount = studentsCount,
                        IsActive = u.active,
                        CreatedAt = profile?.CreatedAt ?? DateTime.Now
                    };
                }).ToList();

                return Ok(ApiResponse<List<ConsultantDto>>.Ok(dtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ConsultantDto>>.Fail($"خطا در دریافت لیست مشاوران: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ConsultantDto>>> GetById(int id)
        {
            var profile = await _context.ConsultantProfiles.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id || p.UserId == id);
            var userId = profile?.UserId ?? id;

            var user = await _context.User.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound(ApiResponse<ConsultantDto>.Fail("مشاور یافت نشد."));

            var studentsCount = await _context.User.CountAsync(s =>
                (s.Rool == UserRoles.Student || s.Rool == "دانش آموز" || s.Rool == "دانش_آموز" || s.Rool == "Student") &&
                s.Id_Moshaver == user.Id);

            var dto = new ConsultantDto
            {
                Id = profile?.Id ?? user.Id,
                UserId = user.Id,
                FullName = user.fname ?? "",
                NationalCode = user.code_meli ?? "",
                PhoneNumber = user.mobile,
                Specialty = profile?.Specialty,
                Bio = profile?.Bio,
                ProfilePictureUrl = user.pic,
                StudentsCount = studentsCount,
                IsActive = user.active,
                CreatedAt = profile?.CreatedAt ?? DateTime.Now
            };

            return Ok(ApiResponse<ConsultantDto>.Ok(dto));
        }

        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.SchoolManager}")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ConsultantDto>>> Create([FromBody] CreateConsultantDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ConsultantDto>.Fail(string.Join(" | ", errors)));
            }

            var trimmedNationalCode = dto.NationalCode.Trim();
            var exists = await _context.User.AnyAsync(u => u.code_meli == trimmedNationalCode);
            if (exists)
                return BadRequest(ApiResponse<ConsultantDto>.Fail("کاربری با این کد ملی قبلاً در سامانه ثبت شده است."));

            var user = new User
            {
                fname = dto.FullName.Trim(),
                code_meli = trimmedNationalCode,
                pass = PasswordHelper.HashPassword(dto.Password),
                mobile = dto.PhoneNumber?.Trim() ?? "",
                gender = dto.Gender ?? "مرد",
                Rool = UserRoles.Consultant,
                active = true
            };

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            var profile = new Shared.Entities.ConsultantProfile
            {
                UserId = user.Id,
                Specialty = dto.Specialty?.Trim(),
                Bio = dto.Bio?.Trim(),
                IsActive = true
            };
            _context.ConsultantProfiles.Add(profile);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("CreateConsultant", "User", user.Id.ToString(), $"ثبت مشاور: {user.fname}");

            return Ok(ApiResponse<ConsultantDto>.Ok(new ConsultantDto
            {
                Id = profile.Id,
                UserId = user.Id,
                FullName = user.fname ?? "",
                NationalCode = user.code_meli ?? "",
                PhoneNumber = user.mobile,
                Specialty = profile.Specialty,
                Bio = profile.Bio,
                IsActive = true,
                CreatedAt = profile.CreatedAt
            }, "مشاور جدید با موفقیت ثبت شد."));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Update(int id, [FromBody] UpdateConsultantDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<bool>.Fail(string.Join(" | ", errors)));
            }

            var profile = await _context.ConsultantProfiles.FirstOrDefaultAsync(p => p.Id == id || p.UserId == id);
            var userId = profile?.UserId ?? id;

            var user = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound(ApiResponse<bool>.Fail("کاربر مشاور یافت نشد."));

            if (!_currentUserService.IsSystemAdmin && user.Id != _currentUserService.UserId)
                return Forbid();

            user.fname = dto.FullName.Trim();
            user.mobile = dto.PhoneNumber?.Trim() ?? user.mobile;
            if (!string.IsNullOrWhiteSpace(dto.Gender)) user.gender = dto.Gender;

            if (_currentUserService.IsSystemAdmin)
            {
                user.active = dto.IsActive;
            }

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
                user.pass = PasswordHelper.HashPassword(dto.NewPassword);

            if (profile == null)
            {
                profile = new Shared.Entities.ConsultantProfile
                {
                    UserId = user.Id,
                    IsActive = user.active
                };
                _context.ConsultantProfiles.Add(profile);
            }

            profile.Specialty = dto.Specialty?.Trim();
            profile.Bio = dto.Bio?.Trim();
            profile.IsActive = user.active;
            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("UpdateConsultant", "User", user.Id.ToString(), $"ویرایش مشاور: {user.fname}");

            return Ok(ApiResponse<bool>.Ok(true, "اطلاعات مشاور به‌روزرسانی شد."));
        }

        [Authorize(Roles = UserRoles.SystemAdmin)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var profile = await _context.ConsultantProfiles.FirstOrDefaultAsync(p => p.Id == id || p.UserId == id);
            var userId = profile?.UserId ?? id;

            var user = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);

            var hasStudents = await _context.User.AnyAsync(s =>
                (s.Rool == UserRoles.Student || s.Rool == "دانش آموز" || s.Rool == "دانش_آموز" || s.Rool == "Student") &&
                s.Id_Moshaver == userId);

            if (hasStudents)
                return BadRequest(ApiResponse<bool>.Fail("این مشاور دارای دانش‌آموزان تحت نظر است. ابتدا دانش‌آموزان را به مشاور دیگر منتقل فرمایید."));

            if (profile != null) _context.ConsultantProfiles.Remove(profile);
            if (user != null) _context.User.Remove(user);

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("DeleteConsultant", "User", userId.ToString(), "حذف مشاور");

            return Ok(ApiResponse<bool>.Ok(true, "مشاور با موفقیت حذف شد."));
        }
    }
}
