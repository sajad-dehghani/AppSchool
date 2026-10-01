using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NovinApp.Server.MyContext;
using NovinApp.Shared.Entities;

namespace NovinApp.Server.Services
{
    public class AuditService : IAuditService
    {
        private readonly MyAppContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(
            MyAppContext context,
            ICurrentUserService currentUserService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _currentUserService = currentUserService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string entityName, string? entityId = null, string? details = null)
        {
            try
            {
                var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

                var auditLog = new AuditLog
                {
                    UserId = _currentUserService.UserId,
                    UserRole = _currentUserService.Role,
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    Details = details,
                    IpAddress = ip,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // لاگین نباید مانع عملیات اصلی کاربر شود
            }
        }
    }
}
