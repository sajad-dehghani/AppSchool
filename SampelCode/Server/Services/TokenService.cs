using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NovinApp.Server.Models;
using NovinApp.Server.MyContext;
using NovinApp.Shared.Constants;
using NovinApp.Shared.DTOs.Auth;
using NovinApp.Shared.Entities;

namespace NovinApp.Server.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MyAppContext _context;

        public TokenService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            MyAppContext context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _context = context;
        }

        public async Task<LoginResponseDto> CreateTokenResponseAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? UserRoles.Student;

            // Load associated profiles if any
            var studentProfile = await _context.StudentProfiles
                .Include(s => s.School)
                .Include(s => s.Consultant)
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            var consultantProfile = await _context.ConsultantProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            var school = await _context.Schools
                .FirstOrDefaultAsync(s => s.ManagerUserId == user.Id);

            int? studentId = studentProfile?.Id;
            int? consultantId = consultantProfile?.Id ?? studentProfile?.ConsultantId;
            int? schoolId = school?.Id ?? studentProfile?.SchoolId;

            // Build Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? user.NationalCode),
                new Claim("FullName", user.FullName ?? ""),
                new Claim("NationalCode", user.NationalCode ?? ""),
                new Claim(ClaimTypes.Role, primaryRole),
                new Claim("Role", primaryRole),
                new Claim("RolePersian", UserRoles.GetPersianTitle(primaryRole))
            };

            if (studentId.HasValue)
                claims.Add(new Claim("StudentId", studentId.Value.ToString()));

            if (consultantId.HasValue)
                claims.Add(new Claim("ConsultantId", consultantId.Value.ToString()));

            if (schoolId.HasValue)
                claims.Add(new Claim("SchoolId", schoolId.Value.ToString()));

            // JWT Signing Key
            var jwtKey = _configuration["jwt:Key"] ?? "NovinAppProductionSecretKeyWithSufficientLength2026!#$";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Access Token: 60 minutes
            var expiresAt = DateTime.UtcNow.AddMinutes(60);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                Issuer = _configuration["jwt:Issuer"] ?? "NovinAppServer",
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            // Refresh Token: 7 days
            var refreshTokenString = GenerateSecureRefreshToken();
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenString,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            var userProfile = new UserProfileDto
            {
                Id = user.Id,
                Username = user.UserName ?? user.NationalCode ?? user.Id.ToString(),
                FullName = user.FullName ?? "",
                NationalCode = user.NationalCode ?? "",
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Role = primaryRole,
                RolePersian = UserRoles.GetPersianTitle(primaryRole),
                StudentId = studentId,
                ConsultantId = consultantId,
                SchoolId = schoolId,
                SchoolName = studentProfile?.School?.Name ?? school?.Name,
                ConsultantName = studentProfile?.Consultant != null ? "مشاور تحصیلی" : null,
                IsActive = user.IsActive
            };

            return new LoginResponseDto
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                ExpiresAt = expiresAt,
                User = userProfile,
                Message = "ورود با موفقیت انجام شد."
            };
        }

        public async Task<LoginResponseDto?> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
                return null;

            var userIdClaim = principal.FindFirst("UserId")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return null;

            var existingRefreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshToken && r.UserId == userId);

            if (existingRefreshToken == null || !existingRefreshToken.IsActive)
                return null;

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || !user.IsActive)
                return null;

            // Revoke current refresh token and replace with new one (Token Rotation)
            existingRefreshToken.IsRevoked = true;

            var newRefreshTokenString = GenerateSecureRefreshToken();
            existingRefreshToken.ReplacedByToken = newRefreshTokenString;

            var response = await CreateTokenResponseAsync(user);
            return response;
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken, int? userId = null)
        {
            var query = _context.RefreshTokens.Where(r => r.Token == refreshToken);
            if (userId.HasValue)
                query = query.Where(r => r.UserId == userId.Value);

            var token = await query.FirstOrDefaultAsync();
            if (token == null)
                return false;

            token.IsRevoked = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwtKey = _configuration["jwt:Key"] ?? "NovinAppProductionSecretKeyWithSufficientLength2026!#$";
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false, // Validate signature even if expired
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        private static string GenerateSecureRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "");
        }
    }
}
