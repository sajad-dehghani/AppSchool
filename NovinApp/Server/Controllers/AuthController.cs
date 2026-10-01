using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NovinApp.Server.Helpers;
using NovinApp.Server.MyContext;
using NovinApp.Shared;
using NovinApp.Shared.Constants;
using NovinApp.Shared.Login;

namespace NovinApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly MyAppContext _context;

        public AuthController(IConfiguration configuration, MyAppContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserToken>> Login([FromBody] UserInfo userInfo)
        {
            if (userInfo == null || string.IsNullOrWhiteSpace(userInfo.Username) || string.IsNullOrWhiteSpace(userInfo.Password))
                return BadRequest("نام کاربری و کلمه عبور الزامی است");

            var trimmedUsername = userInfo.Username.Trim();
            var trimmedPassword = userInfo.Password.Trim();

            // جستجو در جدول User با کد ملی
            var user = await _context.User.FirstOrDefaultAsync(x =>
                x.code_meli == trimmedUsername && x.active == true);

            if (user == null)
                return BadRequest("نام کاربری یا کلمه عبور اشتباه است یا حساب غیرفعال می‌باشد");

            // بررسی رمز عبور
            if (!PasswordHelper.VerifyPassword(trimmedPassword, user.pass))
                return BadRequest("نام کاربری یا کلمه عبور اشتباه است");

            // اگر رمز هنوز هش نشده، هش کن و ذخیره کن
            if (!PasswordHelper.IsHashed(user.pass))
            {
                user.pass = PasswordHelper.HashPassword(trimmedPassword);
                await _context.SaveChangesAsync();
            }

            var token = BuildToken(user);
            return Ok(token);
        }

        private UserToken BuildToken(User user)
        {
            // نرمال‌سازی نقش
            var normalizedRole = UserRoles.NormalizeRole(user.Rool ?? "");

            // ساخت claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.fname ?? user.code_meli ?? user.Id.ToString()),
                new Claim("FullName", user.fname ?? ""),
                new Claim("NationalCode", user.code_meli ?? ""),
                new Claim(ClaimTypes.Role, normalizedRole),
                new Claim("Role", normalizedRole),
                new Claim("RolePersian", UserRoles.GetPersianTitle(normalizedRole))
            };

            // افزودن SchoolId و MoshaverId از User
            if (user.Id_School.HasValue)
                claims.Add(new Claim("SchoolId", user.Id_School.Value.ToString()));

            if (user.Id_Moshaver.HasValue)
                claims.Add(new Claim("ConsultantId", user.Id_Moshaver.Value.ToString()));

            var jwtKey = _configuration["jwt:Key"] ?? "NovinAppSecretKeyMustBeAtLeast32CharsLong12345!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            DateTime expireTime = DateTime.UtcNow.AddHours(8);

            var jwtToken = new JwtSecurityToken(
                issuer: _configuration["jwt:Issuer"],
                audience: null,
                claims: claims,
                expires: expireTime,
                signingCredentials: creds
            );

            return new UserToken
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                ExpireTime = expireTime
            };
        }
    }
}
