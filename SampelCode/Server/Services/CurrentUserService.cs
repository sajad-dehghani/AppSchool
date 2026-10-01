using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NovinApp.Shared.Constants;

namespace NovinApp.Server.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public int? UserId
        {
            get
            {
                var val = User?.FindFirst("UserId")?.Value ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return int.TryParse(val, out var id) ? id : null;
            }
        }

        public string? NationalCode => User?.FindFirst("NationalCode")?.Value;

        public string? FullName => User?.FindFirst("FullName")?.Value ?? User?.FindFirst(ClaimTypes.Name)?.Value;

        public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value ?? User?.FindFirst("Role")?.Value;

        public int? StudentId
        {
            get
            {
                var val = User?.FindFirst("StudentId")?.Value;
                return int.TryParse(val, out var id) ? id : null;
            }
        }

        public int? ConsultantId
        {
            get
            {
                var val = User?.FindFirst("ConsultantId")?.Value;
                return int.TryParse(val, out var id) ? id : null;
            }
        }

        public int? SchoolId
        {
            get
            {
                var val = User?.FindFirst("SchoolId")?.Value;
                return int.TryParse(val, out var id) ? id : null;
            }
        }

        public bool IsSystemAdmin => Role == UserRoles.SystemAdmin;
        public bool IsSchoolManager => Role == UserRoles.SchoolManager;
        public bool IsConsultant => Role == UserRoles.Consultant;
        public bool IsStudent => Role == UserRoles.Student;
    }
}
