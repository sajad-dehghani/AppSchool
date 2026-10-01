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
using NovinApp.Shared.DTOs.Auth;
using NovinApp.Shared.DTOs.Common;
using NovinApp.Shared.Entities;

namespace NovinApp.Server.Controllers
{
    [Authorize(Roles = UserRoles.SystemAdmin)]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IAuditService _auditService;
        private readonly MyAppContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IAuditService auditService,
            MyAppContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _auditService = auditService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<UserProfileDto>>>> GetUsers(
            [FromQuery] string? role = null,
            [FromQuery] string? search = null)
        {
            var query = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(u => u.FullName.Contains(s) || u.NationalCode.Contains(s) || (u.PhoneNumber != null && u.PhoneNumber.Contains(s)));
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var dtos = new List<UserProfileDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var primaryRole = roles.FirstOrDefault() ?? UserRoles.Student;

                if (!string.IsNullOrWhiteSpace(role) && !primaryRole.Equals(role, StringComparison.OrdinalIgnoreCase))
                    continue;

                dtos.Add(new UserProfileDto
                {
                    Id = user.Id,
                    Username = user.UserName ?? user.NationalCode,
                    FullName = user.FullName,
                    NationalCode = user.NationalCode,
                    PhoneNumber = user.PhoneNumber,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    Role = primaryRole,
                    RolePersian = UserRoles.GetPersianTitle(primaryRole),
                    IsActive = user.IsActive
                });
            }

            return Ok(ApiResponse<List<UserProfileDto>>.Ok(dtos));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetUserById(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound(ApiResponse<UserProfileDto>.Fail("کاربر یافت نشد."));

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? UserRoles.Student;

            var dto = new UserProfileDto
            {
                Id = user.Id,
                Username = user.UserName ?? user.NationalCode,
                FullName = user.FullName,
                NationalCode = user.NationalCode,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Role = primaryRole,
                RolePersian = UserRoles.GetPersianTitle(primaryRole),
                IsActive = user.IsActive
            };

            return Ok(ApiResponse<UserProfileDto>.Ok(dto));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> CreateUser([FromBody] RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<UserProfileDto>.Fail("اطلاعات ارسالی نامعتبر است."));

            var trimmedNationalCode = dto.NationalCode.Trim();

            var exists = await _userManager.Users.AnyAsync(u => u.NationalCode == trimmedNationalCode || u.UserName == trimmedNationalCode);
            if (exists)
                return BadRequest(ApiResponse<UserProfileDto>.Fail("کاربری با این کد ملی از قبل ثبت شده است."));

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

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ApiResponse<UserProfileDto>.Fail("خطا در ایجاد کاربر.", errors));
            }

            var normalizedRole = UserRoles.NormalizeRole(dto.Role);
            if (!await _roleManager.RoleExistsAsync(normalizedRole))
            {
                await _roleManager.CreateAsync(new ApplicationRole(normalizedRole, UserRoles.GetPersianTitle(normalizedRole)));
            }

            await _userManager.AddToRoleAsync(user, normalizedRole);

            // اگر نقش دانش‌آموز یا مشاور باشد، رکورد پروفایل مربوطه نیز ساخته شود
            if (normalizedRole == UserRoles.Student)
            {
                var studentProfile = new StudentProfile
                {
                    UserId = user.Id,
                    NationalCode = trimmedNationalCode,
                    FatherName = dto.FatherName?.Trim(),
                    GradeLevel = dto.GradeLevel?.Trim() ?? "پایه دوازدهم",
                    FieldOfStudy = dto.FieldOfStudy?.Trim() ?? "علوم تجربی",
                    SchoolId = dto.SchoolId,
                    ConsultantId = dto.ConsultantId,
                    IsActive = true
                };
                _context.StudentProfiles.Add(studentProfile);
                await _context.SaveChangesAsync();
            }
            else if (normalizedRole == UserRoles.Consultant)
            {
                var consultantProfile = new ConsultantProfile
                {
                    UserId = user.Id,
                    Specialty = "مشاور تحصیلی",
                    IsActive = true
                };
                _context.ConsultantProfiles.Add(consultantProfile);
                await _context.SaveChangesAsync();
            }

            await _auditService.LogAsync("CreateUser", "ApplicationUser", user.Id.ToString(), $"ایجاد کاربر {user.FullName} با نقش {normalizedRole}");

            var userDto = new UserProfileDto
            {
                Id = user.Id,
                Username = user.UserName,
                FullName = user.FullName,
                NationalCode = user.NationalCode,
                PhoneNumber = user.PhoneNumber,
                Role = normalizedRole,
                RolePersian = UserRoles.GetPersianTitle(normalizedRole),
                IsActive = true
            };

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, ApiResponse<UserProfileDto>.Ok(userDto, "کاربر با موفقیت ایجاد شد."));
        }

        [HttpPut("{id}/toggle-status")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleStatus(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound(ApiResponse<bool>.Fail("کاربر یافت نشد."));

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            await _auditService.LogAsync("ToggleUserStatus", "ApplicationUser", id.ToString(), $"تغییر وضعیت فعال‌سازی کاربر {user.FullName} به {user.IsActive}");

            return Ok(ApiResponse<bool>.Ok(user.IsActive, $"وضعیت کاربر به {(user.IsActive ? "فعال" : "غیرفعال")} تغییر یافت."));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound(ApiResponse<bool>.Fail("کاربر یافت نشد."));

            // جلوگیری از حذف ادمین پیش‌فرض
            if (user.UserName == "admin")
                return BadRequest(ApiResponse<bool>.Fail("حذف حساب مدیر ارشد پیش‌فرض سامانه امکان‌پذیر نیست."));

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(ApiResponse<bool>.Fail("خطا در حذف کاربر."));
            }

            await _auditService.LogAsync("DeleteUser", "ApplicationUser", id.ToString(), $"حذف کاربر {user.FullName}");

            return Ok(ApiResponse<bool>.Ok(true, "کاربر با موفقیت حذف شد."));
        }
    }
}
