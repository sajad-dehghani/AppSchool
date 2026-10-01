using System;
using System.Linq;
using System.Security.Claims;
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
using NovinApp.Shared.Login;

namespace NovinApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;
        private readonly MyAppContext _context;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            ICurrentUserService currentUserService,
            IAuditService auditService,
            MyAppContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _currentUserService = currentUserService;
            _auditService = auditService;
            _context = context;
        }

        /// <summary>
        /// ورود استاندارد با نام کاربری یا کد ملی و کلمه عبور
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<LoginResponseDto>.Fail("اطلاعات ورودی نامعتبر است."));

            var trimmedUsername = request.Username.Trim();

            // جستجو بر اساس UserName یا NationalCode
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == trimmedUsername || u.NationalCode == trimmedUsername);

            if (user == null || !user.IsActive)
            {
                return BadRequest(ApiResponse<LoginResponseDto>.Fail("نام کاربری یا کلمه عبور اشتباه است یا حساب غیرفعال می‌باشد."));
            }

            // اعتبارسنجی کلمه عبور از طریق Identity PasswordHasher
            var passwordCheck = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordCheck)
            {
                // لاگ تلاش ناموفق
                await _auditService.LogAsync("FailedLoginAttempt", "User", user.Id.ToString(), $"کد ملی: {user.NationalCode}");
                return BadRequest(ApiResponse<LoginResponseDto>.Fail("نام کاربری یا کلمه عبور اشتباه است."));
            }

            // به‌روزرسانی زمان آخرین ورود
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // ایجاد پاسخ توکن
            var tokenResponse = await _tokenService.CreateTokenResponseAsync(user);

            // ثبت لاگ ورود موفق
            await _auditService.LogAsync("LoginSuccess", "User", user.Id.ToString(), $"ورود کاربر: {user.FullName} ({tokenResponse.User.Role})");

            return Ok(ApiResponse<LoginResponseDto>.Ok(tokenResponse, "ورود با موفقیت انجام شد."));
        }


        /// <summary>
        /// تمدید خودکار توکن منقضی‌شده با Refresh Token معتبر
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<LoginResponseDto>.Fail("توکن‌های ارسالی نامعتبر هستند."));

            var tokenResponse = await _tokenService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);
            if (tokenResponse == null)
            {
                return Unauthorized(ApiResponse<LoginResponseDto>.Fail("نشست کاربری شما منقضی شده است. لطفاً مجدداً وارد شوید."));
            }

            return Ok(ApiResponse<LoginResponseDto>.Ok(tokenResponse, "توکن با موفقیت تمدید شد."));
        }

        /// <summary>
        /// دریافت مشخصات و پروفایل کامل کاربر جاری
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetCurrentUserProfile()
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<UserProfileDto>.Fail("کاربر احراز هویت نشده است."));

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null)
                return NotFound(ApiResponse<UserProfileDto>.Fail("کاربر یافت نشد."));

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? UserRoles.Student;

            var studentProfile = await _context.StudentProfiles
                .Include(s => s.School)
                .Include(s => s.Consultant)
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            var consultantProfile = await _context.ConsultantProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            var school = await _context.Schools
                .FirstOrDefaultAsync(s => s.ManagerUserId == user.Id);

            var profile = new UserProfileDto
            {
                Id = user.Id,
                Username = user.UserName ?? user.NationalCode,
                FullName = user.FullName,
                NationalCode = user.NationalCode,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Role = primaryRole,
                RolePersian = UserRoles.GetPersianTitle(primaryRole),
                StudentId = studentProfile?.Id,
                ConsultantId = consultantProfile?.Id ?? studentProfile?.ConsultantId,
                SchoolId = school?.Id ?? studentProfile?.SchoolId,
                SchoolName = studentProfile?.School?.Name ?? school?.Name,
                ConsultantName = studentProfile?.Consultant != null ? "مشاور تحصیلی" : null,
                IsActive = user.IsActive
            };

            return Ok(ApiResponse<UserProfileDto>.Ok(profile));
        }

        /// <summary>
        /// ابطال نشست و خروج امن از سیستم
        /// </summary>
        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<ActionResult<ApiResponse<bool>>> RevokeToken([FromBody] string refreshToken)
        {
            var userId = _currentUserService.UserId;
            var success = await _tokenService.RevokeTokenAsync(refreshToken, userId);

            await _auditService.LogAsync("RevokeToken", "RefreshToken", null, $"کاربر {userId} نشست خود را باطل کرد.");

            return Ok(ApiResponse<bool>.Ok(success, "نشست با موفقیت باطل شد."));
        }

        /// <summary>
        /// تغییر امن کلمه عبور با استفاده از Identity
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<bool>.Fail("اطلاعات تغییر کلمه عبور نامعتبر است."));

            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<bool>.Fail("عدم دسترسی."));

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null)
                return NotFound(ApiResponse<bool>.Fail("کاربر یافت نشد."));

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ApiResponse<bool>.Fail("کلمه عبور فعلی نادرست است یا کلمه عبور جدید الزامات امنیتی را برآورده نمی‌کند.", errors));
            }

            await _auditService.LogAsync("ChangePassword", "User", user.Id.ToString(), "کلمه عبور با موفقیت تغییر یافت.");

            return Ok(ApiResponse<bool>.Ok(true, "کلمه عبور با موفقیت تغییر یافت."));
        }
    }
}
