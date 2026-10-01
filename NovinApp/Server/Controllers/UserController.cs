using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovinApp.Server.Helpers;
using NovinApp.Server.MyContext;
using NovinApp.Server.Services;
using NovinApp.Shared;
using NovinApp.Shared.Constants;
using NovinApp.Shared.DTOs.Common;
using NovinApp.Shared.DTOs.Users;
using NovinApp.Shared.Entities;

namespace WebApp.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly MyAppContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;

        public UserController(MyAppContext context, ICurrentUserService currentUserService, IAuditService auditService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _auditService = auditService;
        }

        /// <summary>
        /// دریافت آمار کلی و شاخص‌های آماری کاربران به صورت کاملاً مقیاس‌پذیر
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<UserStatsDto>>> GetStats()
        {
            try
            {
                var query = _context.User.AsNoTracking();

                if (_currentUserService.IsSchoolManager && !_currentUserService.IsSystemAdmin)
                {
                    var schId = _currentUserService.SchoolId ?? 0;
                    query = query.Where(u => u.Id_School == schId);
                }

                var totalUsers = await query.CountAsync();
                var totalStudents = await query.CountAsync(u => u.Rool == UserRoles.Student || u.Rool == "دانش آموز" || u.Rool == "دانش_آموز" || u.Rool == "Student");
                var totalConsultants = await query.CountAsync(u => u.Rool == UserRoles.Consultant || u.Rool == "مشاور" || u.Rool == "Moshaver");
                var totalSchoolManagers = await query.CountAsync(u => u.Rool == UserRoles.SchoolManager || u.Rool == "مدیر مدرسه" || u.Rool == "مدیر" || u.Rool == "SchoolManager");
                var totalAdmins = await query.CountAsync(u => u.Rool == UserRoles.SystemAdmin || u.Rool == "مدیر سیستم" || u.Rool == "SystemAdmin" || u.Rool == "ادمین");

                var studentsWithoutConsultant = await query.CountAsync(u =>
                    (u.Rool == UserRoles.Student || u.Rool == "دانش آموز" || u.Rool == "دانش_آموز" || u.Rool == "Student") &&
                    (!u.Id_Moshaver.HasValue || u.Id_Moshaver.Value <= 0 || u.Moshaver == "بدون مشاور" || string.IsNullOrEmpty(u.Moshaver)));

                var studentsWithoutSchool = await query.CountAsync(u =>
                    (u.Rool == UserRoles.Student || u.Rool == "دانش آموز" || u.Rool == "دانش_آموز" || u.Rool == "Student") &&
                    (!u.Id_School.HasValue || u.Id_School.Value <= 0));

                var inactiveUsers = await query.CountAsync(u => !u.active);

                var stats = new UserStatsDto
                {
                    TotalUsers = totalUsers,
                    TotalStudents = totalStudents,
                    TotalConsultants = totalConsultants,
                    TotalSchoolManagers = totalSchoolManagers,
                    TotalAdmins = totalAdmins,
                    StudentsWithoutConsultant = studentsWithoutConsultant,
                    StudentsWithoutSchool = studentsWithoutSchool,
                    InactiveUsers = inactiveUsers
                };

                return Ok(ApiResponse<UserStatsDto>.Ok(stats));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserStatsDto>.Fail($"خطا در محاسبه آمار کاربران: {ex.Message}"));
            }
        }

        /// <summary>
        /// جستجو و واکشی صفحه‌بندی‌شده و فیلترشده کاربران در مقیاس بسیار بالا
        /// </summary>
        [HttpPost("paged")]
        public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> GetPagedUsers([FromBody] UserFilterDto filter)
        {
            try
            {
                var query = _context.User.AsNoTracking();

                // محدودیت دسترسی مدیر مدرسه
                if (_currentUserService.IsSchoolManager && !_currentUserService.IsSystemAdmin)
                {
                    var schId = _currentUserService.SchoolId ?? 0;
                    query = query.Where(u => u.Id_School == schId);
                }

                // فیلتر نقش
                if (!string.IsNullOrWhiteSpace(filter.Role) && filter.Role != "all" && filter.Role != "همه")
                {
                    var normRole = UserRoles.NormalizeRole(filter.Role);
                    if (normRole == UserRoles.Student)
                    {
                        query = query.Where(u => u.Rool == UserRoles.Student || u.Rool == "دانش آموز" || u.Rool == "دانش_آموز" || u.Rool == "Student");
                    }
                    else if (normRole == UserRoles.Consultant)
                    {
                        query = query.Where(u => u.Rool == UserRoles.Consultant || u.Rool == "مشاور" || u.Rool == "Moshaver");
                    }
                    else if (normRole == UserRoles.SchoolManager)
                    {
                        query = query.Where(u => u.Rool == UserRoles.SchoolManager || u.Rool == "مدیر مدرسه" || u.Rool == "مدیر" || u.Rool == "SchoolManager");
                    }
                    else if (normRole == UserRoles.SystemAdmin)
                    {
                        query = query.Where(u => u.Rool == UserRoles.SystemAdmin || u.Rool == "مدیر سیستم" || u.Rool == "SystemAdmin" || u.Rool == "ادمین");
                    }
                    else
                    {
                        query = query.Where(u => u.Rool == filter.Role || u.Rool == normRole);
                    }
                }

                // فیلتر مدرسه
                if (filter.SchoolId.HasValue && filter.SchoolId.Value > 0)
                {
                    query = query.Where(u => u.Id_School == filter.SchoolId.Value);
                }

                // فیلتر مشاور
                if (filter.ConsultantId.HasValue && filter.ConsultantId.Value > 0)
                {
                    query = query.Where(u => u.Id_Moshaver == filter.ConsultantId.Value);
                }

                // فیلتر بدون مشاور
                if (filter.WithoutConsultantOnly == true)
                {
                    query = query.Where(u => !u.Id_Moshaver.HasValue || u.Id_Moshaver.Value <= 0 || u.Moshaver == "بدون مشاور" || string.IsNullOrEmpty(u.Moshaver));
                }

                // فیلتر بدون مدرسه
                if (filter.WithoutSchoolOnly == true)
                {
                    query = query.Where(u => !u.Id_School.HasValue || u.Id_School.Value <= 0);
                }

                // فیلتر وضعیت
                if (filter.IsActive.HasValue)
                {
                    query = query.Where(u => u.active == filter.IsActive.Value);
                }

                // جستجوی متنی
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var term = filter.SearchTerm.Trim();
                    query = query.Where(u =>
                        (u.fname != null && u.fname.Contains(term)) ||
                        (u.code_meli != null && u.code_meli.Contains(term)) ||
                        (u.mobile != null && u.mobile.Contains(term)));
                }

                var totalCount = await query.CountAsync();

                var pageNumber = Math.Max(1, filter.PageNumber);
                var pageSize = Math.Clamp(filter.PageSize, 5, 200);

                var items = await query
                    .OrderByDescending(u => u.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var schoolIds = items.Where(u => u.Id_School.HasValue).Select(u => u.Id_School!.Value).Distinct().ToList();
                var schools = await _context.Schools.AsNoTracking()
                    .Where(s => schoolIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id, s => s.Name);

                var consultantIds = items.Where(u => u.Id_Moshaver.HasValue).Select(u => u.Id_Moshaver!.Value).Distinct().ToList();
                var consultants = await _context.User.AsNoTracking()
                    .Where(u => consultantIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.fname);

                var dtos = items.Select(u =>
                {
                    string? sName = null;
                    if (u.Id_School.HasValue && schools.TryGetValue(u.Id_School.Value, out var scName))
                        sName = scName;

                    string cName = "بدون مشاور";
                    if (!string.IsNullOrWhiteSpace(u.Moshaver) && u.Moshaver != "بدون مشاور")
                        cName = u.Moshaver;
                    else if (u.Id_Moshaver.HasValue && consultants.TryGetValue(u.Id_Moshaver.Value, out var cn))
                        cName = cn ?? "بدون مشاور";

                    return new UserDto
                    {
                        Id = u.Id,
                        FullName = u.fname ?? "",
                        NationalCode = u.code_meli ?? "",
                        PhoneNumber = u.mobile,
                        Gender = u.gender ?? "مرد",
                        Role = UserRoles.NormalizeRole(u.Rool),
                        RolePersian = UserRoles.GetPersianTitle(u.Rool),
                        SchoolId = u.Id_School,
                        SchoolName = sName,
                        ConsultantId = u.Id_Moshaver,
                        ConsultantName = cName,
                        ProfilePictureUrl = u.pic,
                        IsActive = u.active
                    };
                }).ToList();

                var result = new PagedResult<UserDto>(dtos, totalCount, pageNumber, pageSize);
                return Ok(ApiResponse<PagedResult<UserDto>>.Ok(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResult<UserDto>>.Fail($"خطا در دریافت لیست کاربران: {ex.Message}"));
            }
        }

        /// <summary>
        /// تخصیص گروهی مدرسه به چند کاربر
        /// </summary>
        [Authorize(Roles = UserRoles.SystemAdmin)]
        [HttpPost("batch-assign-school")]
        public async Task<ActionResult<ApiResponse<bool>>> BatchAssignSchool([FromBody] BatchAssignSchoolDto dto)
        {
            if (dto.UserIds == null || !dto.UserIds.Any())
                return BadRequest(ApiResponse<bool>.Fail("هیچ کاربری برای عملیات گروهی انتخاب نشده است."));

            var users = await _context.User.Where(u => dto.UserIds.Contains(u.Id)).ToListAsync();
            foreach (var u in users)
            {
                u.Id_School = dto.SchoolId;
            }

            var studentProfiles = await _context.StudentProfiles.Where(p => dto.UserIds.Contains(p.UserId)).ToListAsync();
            foreach (var sp in studentProfiles)
            {
                sp.SchoolId = dto.SchoolId;
                sp.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("BatchAssignSchool", "User", $"{users.Count} users", $"تخصیص گروهی مدرسه به {users.Count} کاربر");

            return Ok(ApiResponse<bool>.Ok(true, $"مدرسه برای {users.Count} کاربر با موفقیت ثبت گردید."));
        }

        /// <summary>
        /// تخصیص گروهی مشاور به چند دانش‌آموز
        /// </summary>
        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.SchoolManager}")]
        [HttpPost("batch-assign-consultant")]
        public async Task<ActionResult<ApiResponse<bool>>> BatchAssignConsultant([FromBody] BatchAssignConsultantDto dto)
        {
            if (dto.UserIds == null || !dto.UserIds.Any())
                return BadRequest(ApiResponse<bool>.Fail("هیچ کاربری برای عملیات گروهی انتخاب نشده است."));

            string consultantName = "بدون مشاور";
            int? consultantUserId = null;
            int? consultantProfileId = null;

            if (dto.ConsultantId.HasValue && dto.ConsultantId.Value > 0)
            {
                var cons = await _context.User.FirstOrDefaultAsync(c => c.Id == dto.ConsultantId.Value);
                if (cons != null)
                {
                    consultantUserId = cons.Id;
                    consultantName = cons.fname ?? "بدون مشاور";

                    var cp = await _context.ConsultantProfiles.FirstOrDefaultAsync(p => p.UserId == cons.Id || p.Id == dto.ConsultantId.Value);
                    if (cp == null)
                    {
                        cp = new ConsultantProfile { UserId = cons.Id, IsActive = true, CreatedAt = DateTime.UtcNow };
                        _context.ConsultantProfiles.Add(cp);
                        await _context.SaveChangesAsync();
                    }
                    consultantProfileId = cp.Id;
                }
            }

            var users = await _context.User.Where(u => dto.UserIds.Contains(u.Id)).ToListAsync();
            foreach (var u in users)
            {
                u.Id_Moshaver = consultantUserId;
                u.Moshaver = consultantName;
            }

            var studentProfiles = await _context.StudentProfiles.Where(p => dto.UserIds.Contains(p.UserId)).ToListAsync();
            foreach (var sp in studentProfiles)
            {
                sp.ConsultantId = consultantProfileId;
                sp.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("BatchAssignConsultant", "User", $"{users.Count} users", $"تخصیص گروهی مشاور ({consultantName}) به {users.Count} کاربر");

            return Ok(ApiResponse<bool>.Ok(true, $"مشاور برای {users.Count} کاربر با موفقیت تغییر یافت."));
        }

        /// <summary>
        /// فعال‌سازی یا غیرفعال‌سازی دسته‌جمعی کاربران
        /// </summary>
        [Authorize(Roles = UserRoles.SystemAdmin)]
        [HttpPost("batch-toggle-status")]
        public async Task<ActionResult<ApiResponse<bool>>> BatchToggleStatus([FromBody] BatchToggleStatusDto dto)
        {
            if (dto.UserIds == null || !dto.UserIds.Any())
                return BadRequest(ApiResponse<bool>.Fail("هیچ کاربری انتخاب نشده است."));

            var users = await _context.User.Where(u => dto.UserIds.Contains(u.Id)).ToListAsync();
            foreach (var u in users)
            {
                u.active = dto.IsActive;
            }

            var studentProfiles = await _context.StudentProfiles.Where(p => dto.UserIds.Contains(p.UserId)).ToListAsync();
            foreach (var sp in studentProfiles)
            {
                sp.IsActive = dto.IsActive;
                sp.UpdatedAt = DateTime.UtcNow;
            }

            var consultantProfiles = await _context.ConsultantProfiles.Where(p => dto.UserIds.Contains(p.UserId)).ToListAsync();
            foreach (var cp in consultantProfiles)
            {
                cp.IsActive = dto.IsActive;
                cp.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, $"وضعیت {users.Count} کاربر با موفقیت به‌روزرسانی شد."));
        }

        /// <summary>
        /// حذف گروهی کاربران
        /// </summary>
        [Authorize(Roles = UserRoles.SystemAdmin)]
        [HttpPost("batch-delete")]
        public async Task<ActionResult<ApiResponse<bool>>> BatchDelete([FromBody] BatchDeleteUsersDto dto)
        {
            if (dto.UserIds == null || !dto.UserIds.Any())
                return BadRequest(ApiResponse<bool>.Fail("هیچ کاربری انتخاب نشده است."));

            // جلوگیری از حذف اکانت ادمین جاری
            var currentUserId = _currentUserService.UserId ?? 0;
            var targetIds = dto.UserIds.Where(id => id != currentUserId).ToList();

            var users = await _context.User.Where(u => targetIds.Contains(u.Id)).ToListAsync();
            var studentProfiles = await _context.StudentProfiles.Where(p => targetIds.Contains(p.UserId)).ToListAsync();
            var consultantProfiles = await _context.ConsultantProfiles.Where(p => targetIds.Contains(p.UserId)).ToListAsync();

            _context.StudentProfiles.RemoveRange(studentProfiles);
            _context.ConsultantProfiles.RemoveRange(consultantProfiles);
            _context.User.RemoveRange(users);

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("BatchDelete", "User", $"{users.Count} users", $"حذف گروهی {users.Count} کاربر");

            return Ok(ApiResponse<bool>.Ok(true, $"{users.Count} کاربر با موفقیت حذف گردیدند."));
        }

        /// <summary>
        /// ایمپورت فوق‌العاده سریع و دسته‌جمعی هزاران کاربر (Bulk Import)
        /// </summary>
        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.SchoolManager}")]
        [HttpPost("bulk-import")]
        public async Task<ActionResult<ApiResponse<BulkImportResultDto>>> BulkImport([FromBody] BulkImportRequestDto request)
        {
            if (request.Users == null || !request.Users.Any())
                return BadRequest(ApiResponse<BulkImportResultDto>.Fail("لیست داده‌ها خالی است."));

            var result = new BulkImportResultDto
            {
                TotalRows = request.Users.Count
            };

            var existingNationalCodes = await _context.User.AsNoTracking()
                .Where(u => u.code_meli != null)
                .Select(u => u.code_meli!)
                .ToHashSetAsync();

            var schools = await _context.Schools.AsNoTracking().ToDictionaryAsync(s => s.Name, s => s.Id);
            var consultants = await _context.User.AsNoTracking()
                .Where(u => u.Rool == UserRoles.Consultant || u.Rool == "مشاور" || u.Rool == "Moshaver")
                .ToDictionaryAsync(u => u.fname ?? "", u => u.Id);

            var newUsers = new List<User>();
            var newProfiles = new List<StudentProfile>();

            int rowNumber = 0;
            foreach (var item in request.Users)
            {
                rowNumber++;
                var nationalCode = item.NationalCode?.Trim();
                if (string.IsNullOrWhiteSpace(nationalCode))
                {
                    result.FailedCount++;
                    result.ErrorMessages.Add($"ردیف {rowNumber}: کد ملی خالی است.");
                    continue;
                }

                if (existingNationalCodes.Contains(nationalCode))
                {
                    result.SkippedCount++;
                    continue;
                }

                int? targetSchoolId = item.SchoolId ?? request.DefaultSchoolId;
                if (!targetSchoolId.HasValue && !string.IsNullOrWhiteSpace(item.SchoolName) && schools.TryGetValue(item.SchoolName.Trim(), out var sid))
                {
                    targetSchoolId = sid;
                }

                int? targetConsultantId = item.ConsultantId ?? request.DefaultConsultantId;
                string consultantName = "بدون مشاور";
                if (!targetConsultantId.HasValue && !string.IsNullOrWhiteSpace(item.ConsultantName) && consultants.TryGetValue(item.ConsultantName.Trim(), out var cid))
                {
                    targetConsultantId = cid;
                    consultantName = item.ConsultantName.Trim();
                }

                var rawPass = string.IsNullOrWhiteSpace(item.Password) ? nationalCode : item.Password.Trim();
                var user = new User
                {
                    fname = string.IsNullOrWhiteSpace(item.FullName) ? "کاربر جدید" : item.FullName.Trim(),
                    code_meli = nationalCode,
                    pass = PasswordHelper.HashPassword(rawPass),
                    mobile = item.PhoneNumber?.Trim() ?? "",
                    gender = item.Gender?.Trim() ?? "مرد",
                    Rool = string.IsNullOrWhiteSpace(item.Role) ? request.DefaultRole : UserRoles.NormalizeRole(item.Role),
                    Id_School = targetSchoolId,
                    Id_Moshaver = targetConsultantId,
                    Moshaver = consultantName,
                    active = true
                };

                newUsers.Add(user);
                existingNationalCodes.Add(nationalCode);
                result.SuccessCount++;
            }

            if (newUsers.Any())
            {
                await _context.User.AddRangeAsync(newUsers);
                await _context.SaveChangesAsync();

                // ساخت پروفایل‌های دانش‌آموزی برای کاربران با نقش دانش‌آموز
                foreach (var u in newUsers.Where(x => x.Rool == UserRoles.Student || x.Rool == "دانش آموز"))
                {
                    newProfiles.Add(new StudentProfile
                    {
                        UserId = u.Id,
                        NationalCode = u.code_meli ?? "",
                        SchoolId = u.Id_School,
                        IsActive = true,
                        GradeLevel = "پایه دوازدهم",
                        FieldOfStudy = "علوم تجربی",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (newProfiles.Any())
                {
                    await _context.StudentProfiles.AddRangeAsync(newProfiles);
                    await _context.SaveChangesAsync();
                }

                await _auditService.LogAsync("BulkImportUsers", "User", $"{newUsers.Count} users", $"ایمپورت دسته‌جمعی {newUsers.Count} کاربر");
            }

            return Ok(ApiResponse<BulkImportResultDto>.Ok(result, $"عملیات با موفقیت انجام شد: {result.SuccessCount} ثبت شد، {result.SkippedCount} تکراری نادیده گرفته شد، {result.FailedCount} خطا."));
        }

        // === Endpoint های قبلی جهت حفظ کامل سازگاری سیستم ===

        [HttpGet("all")]
        public async Task<IActionResult> Get_all()
        {
            var devs = await _context.User.AsNoTracking().ToListAsync();
            return Ok(devs);
        }

        [HttpGet("moshaver")]
        public async Task<IActionResult> Get_moshaver()
        {
            var devs = await _context.User.AsNoTracking()
                .Where(x => x.Rool == "مشاور" || x.Rool == "Consultant" || x.Rool == "Moshaver")
                .ToListAsync();
            return Ok(devs);
        }

        [HttpGet("students")]
        public async Task<IActionResult> Get_students()
        {
            var devs = await _context.User.AsNoTracking()
                .Where(x => x.Rool == "دانش آموز" || x.Rool == "دانش_آموز" || x.Rool == "Student")
                .ToListAsync();
            return Ok(devs);
        }

        [HttpGet("admins")]
        public async Task<IActionResult> Get_admins()
        {
            var devs = await _context.User.AsNoTracking()
                .Where(x => x.Rool == "مدیر سیستم" || x.Rool == "SystemAdmin" || x.Rool == "ادمین")
                .ToListAsync();
            return Ok(devs);
        }

        [HttpGet("login/{id}")]
        public async Task<IActionResult> GetLoginUser(int id)
        {
            var dev = await _context.User.FirstOrDefaultAsync(d => d.Id == id);
            if (dev != null)
                return Ok(dev);
            else
                return NotFound();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, [FromQuery] bool? mode = null)
        {
            var dev = await _context.User.FirstOrDefaultAsync(d => d.Id == id);
            if (dev != null)
                return Ok(dev);
            else
                return NotFound();
        }

        [HttpGet("CodeMeli/{code}")]
        public async Task<IActionResult> Get_code_meli(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest();

            var dev = await _context.User.Where(x => x.code_meli == code.Trim()).ToListAsync();
            if (dev != null && dev.Any())
                return Ok(dev);
            else
                return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Post(User personel)
        {
            if (personel == null)
                return BadRequest();

            if (!string.IsNullOrWhiteSpace(personel.pass) && !PasswordHelper.IsHashed(personel.pass))
            {
                personel.pass = PasswordHelper.HashPassword(personel.pass.Trim());
            }

            _context.User.Add(personel);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> Put(User personel)
        {
            if (personel == null)
                return BadRequest();

            var existing = await _context.User.AsNoTracking().FirstOrDefaultAsync(x => x.Id == personel.Id);
            if (existing != null)
            {
                // فقط مدیر سیستم مجاز به تغییر کد ملی کاربران است
                if (!_currentUserService.IsSystemAdmin)
                {
                    personel.code_meli = existing.code_meli;
                    personel.Rool = existing.Rool; // نقش نیز فقط توسط مدیر سیستم قابل تغییر است
                }

                if (!string.IsNullOrWhiteSpace(personel.pass) && personel.pass != existing.pass && !PasswordHelper.IsHashed(personel.pass))
                {
                    personel.pass = PasswordHelper.HashPassword(personel.pass.Trim());
                }
                else if (string.IsNullOrWhiteSpace(personel.pass))
                {
                    personel.pass = existing.pass;
                }
            }

            _context.User.Update(personel);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var dev = await _context.User.FirstOrDefaultAsync(d => d.Id == id);
            if (dev == null)
                return NotFound();

            var studentProfiles = await _context.StudentProfiles.Where(p => p.UserId == id).ToListAsync();
            var consultantProfiles = await _context.ConsultantProfiles.Where(p => p.UserId == id).ToListAsync();

            _context.StudentProfiles.RemoveRange(studentProfiles);
            _context.ConsultantProfiles.RemoveRange(consultantProfiles);
            _context.User.Remove(dev);

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
