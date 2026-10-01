using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NovinApp.Server.MyContext;
using NovinApp.Shared;
using NovinApp.Shared.Login;

namespace NovinApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly MyAppContext _context;
        private readonly HttpClient _httpClient;

        public AuthController(IConfiguration configuration, MyAppContext context, HttpClient httpClient)
        {
            _configuration = configuration;
            _context = context;
            _httpClient = httpClient;
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserToken>> Login([FromBody] UserInfo userInfo)
        {
            if (userInfo == null || string.IsNullOrWhiteSpace(userInfo.Username) || string.IsNullOrWhiteSpace(userInfo.Password))
                return BadRequest("نام کاربری و کلمه عبور الزامی است");

            var trimmedUsername = userInfo.Username.Trim();
            var trimmedPassword = userInfo.Password.Trim();

            var dev = await _context.User
                .FirstOrDefaultAsync(x => x.code_meli == trimmedUsername && x.pass == trimmedPassword && x.active == true);

            if (dev == null)
                return BadRequest("نام کاربری یا کلمه عبور اشتباه است یا حساب غیرفعال می‌باشد");

            var token = BuildToken(dev);

            return Ok(token);
        }

        /// <summary>
        /// ساخت JWT Token با کلیم‌های کامل
        /// </summary>
        private UserToken BuildToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.fname ?? user.code_meli ?? user.Id.ToString()),
                new Claim("FullName", user.fname ?? ""),
                new Claim("NationalCode", user.code_meli ?? ""),
                new Claim(ClaimTypes.Role, string.IsNullOrWhiteSpace(user.Rool) ? "دانش آموز" : user.Rool),
                new Claim("Role", string.IsNullOrWhiteSpace(user.Rool) ? "دانش آموز" : user.Rool)
            };

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

            string tokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return new UserToken
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                token = tokenString,
                ExpireTime = expireTime
            };
        }
    }
}
