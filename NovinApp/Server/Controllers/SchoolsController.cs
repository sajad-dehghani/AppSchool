using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovinApp.Server.MyContext;
using NovinApp.Server.Services;
using NovinApp.Shared.Constants;
using NovinApp.Shared.DTOs.Common;
using NovinApp.Shared.DTOs.Schools;
using NovinApp.Shared.Entities;

namespace NovinApp.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolsController : ControllerBase
    {
        private readonly MyAppContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;

        public SchoolsController(MyAppContext context, ICurrentUserService currentUserService, IAuditService auditService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<SchoolDto>>>> GetAll()
        {
            var query = _context.Schools.AsNoTracking();

            if (_currentUserService.IsSchoolManager && _currentUserService.SchoolId.HasValue)
                query = query.Where(s => s.Id == _currentUserService.SchoolId.Value);
            else if (!_currentUserService.IsSystemAdmin)
                query = query.Where(s => s.IsActive);

            var schools = await query
                .OrderBy(s => s.Name)
                .Select(s => new SchoolDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    SchoolCode = s.SchoolCode,
                    RegionName = s.RegionName,
                    Address = s.Address,
                    PhoneNumber = s.PhoneNumber,
                    ManagerUserId = s.ManagerUserId,
                    StudentsCount = s.Students.Count,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<SchoolDto>>.Ok(schools));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SchoolDto>>> GetById(int id)
        {
            if (_currentUserService.IsSchoolManager && _currentUserService.SchoolId != id)
                return Forbid();

            var s = await _context.Schools.AsNoTracking()
                .Include(x => x.Students)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (s == null)
                return NotFound(ApiResponse<SchoolDto>.Fail("مدرسه مورد نظر یافت نشد."));

            var dto = new SchoolDto
            {
                Id = s.Id, Name = s.Name, SchoolCode = s.SchoolCode,
                RegionName = s.RegionName, Address = s.Address, PhoneNumber = s.PhoneNumber,
                ManagerUserId = s.ManagerUserId, StudentsCount = s.Students.Count,
                IsActive = s.IsActive, CreatedAt = s.CreatedAt
            };

            return Ok(ApiResponse<SchoolDto>.Ok(dto));
        }

        [Authorize(Roles = UserRoles.SystemAdmin)]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<SchoolDto>>> Create([FromBody] CreateSchoolDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<SchoolDto>.Fail("اطلاعات ورودی نامعتبر است."));

            var school = new School
            {
                Name = dto.Name.Trim(),
                SchoolCode = dto.SchoolCode,
                RegionName = dto.RegionName?.Trim(),
                Address = dto.Address?.Trim(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                ManagerUserId = dto.ManagerUserId,
                IsActive = true
            };

            _context.Schools.Add(school);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("CreateSchool", "School", school.Id.ToString(), $"ایجاد مدرسه: {school.Name}");

            var resultDto = new SchoolDto
            {
                Id = school.Id, Name = school.Name, SchoolCode = school.SchoolCode,
                RegionName = school.RegionName, Address = school.Address, PhoneNumber = school.PhoneNumber,
                ManagerUserId = school.ManagerUserId, IsActive = school.IsActive, CreatedAt = school.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = school.Id }, ApiResponse<SchoolDto>.Ok(resultDto, "مدرسه با موفقیت ثبت شد."));
        }

        [Authorize(Roles = UserRoles.SystemAdmin)]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Update(int id, [FromBody] UpdateSchoolDto dto)
        {
            if (id != dto.Id || !ModelState.IsValid)
                return BadRequest(ApiResponse<bool>.Fail("اطلاعات ارسالی نامعتبر است."));

            var school = await _context.Schools.FindAsync(id);
            if (school == null)
                return NotFound(ApiResponse<bool>.Fail("مدرسه یافت نشد."));

            school.Name = dto.Name.Trim();
            school.SchoolCode = dto.SchoolCode;
            school.RegionName = dto.RegionName?.Trim();
            school.Address = dto.Address?.Trim();
            school.PhoneNumber = dto.PhoneNumber?.Trim();
            school.ManagerUserId = dto.ManagerUserId;
            school.IsActive = dto.IsActive;
            school.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("UpdateSchool", "School", school.Id.ToString(), $"ویرایش مدرسه: {school.Name}");

            return Ok(ApiResponse<bool>.Ok(true, "اطلاعات مدرسه با موفقیت به‌روزرسانی شد."));
        }

        [Authorize(Roles = UserRoles.SystemAdmin)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var school = await _context.Schools.Include(s => s.Students).FirstOrDefaultAsync(s => s.Id == id);
            if (school == null)
                return NotFound(ApiResponse<bool>.Fail("مدرسه یافت نشد."));

            if (school.Students.Any())
                return BadRequest(ApiResponse<bool>.Fail("این مدرسه دارای دانش‌آموزان است. ابتدا دانش‌آموزان را منتقل فرمایید."));

            _context.Schools.Remove(school);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("DeleteSchool", "School", id.ToString(), $"حذف مدرسه: {school.Name}");

            return Ok(ApiResponse<bool>.Ok(true, "مدرسه با موفقیت حذف شد."));
        }
    }
}
