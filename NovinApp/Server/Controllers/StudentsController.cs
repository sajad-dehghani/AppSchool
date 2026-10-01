using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovinApp.Server.Helpers;
using NovinApp.Server.MyContext;
using NovinApp.Server.Services;
using NovinApp.Shared;
using NovinApp.Shared.Constants;
using NovinApp.Shared.DTOs.Common;
using NovinApp.Shared.DTOs.Students;
using NovinApp.Shared.Entities;

namespace NovinApp.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly MyAppContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;

        public StudentsController(MyAppContext context, ICurrentUserService currentUserService, IAuditService auditService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<StudentDto>>>> GetAll(
            [FromQuery] int? schoolId = null,
            [FromQuery] int? consultantId = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.User.AsNoTracking().Where(u =>
                    u.Rool == UserRoles.Student ||
                    u.Rool == "دانش آموز" ||
                    u.Rool == "دانش_آموز" ||
                    u.Rool == "Student");

                if (_currentUserService.IsStudent)
                {
                    query = query.Where(s => s.Id == _currentUserService.UserId);
                }
                else if (_currentUserService.IsConsultant && !_currentUserService.IsSystemAdmin)
                {
                    var currentConsId = _currentUserService.UserId;
                    query = query.Where(s => s.Id_Moshaver == currentConsId);
                }
                else if (_currentUserService.IsSchoolManager && !_currentUserService.IsSystemAdmin)
                {
                    var currentSchId = _currentUserService.SchoolId;
                    if (currentSchId.HasValue)
                        query = query.Where(s => s.Id_School == currentSchId.Value);
                }
                else if (_currentUserService.IsSystemAdmin)
                {
                    if (schoolId.HasValue && schoolId.Value > 0)
                        query = query.Where(s => s.Id_School == schoolId.Value);

                    if (consultantId.HasValue && consultantId.Value > 0)
                        query = query.Where(s => s.Id_Moshaver == consultantId.Value);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim();
                    query = query.Where(x =>
                        (x.fname != null && x.fname.Contains(s)) ||
                        (x.code_meli != null && x.code_meli.Contains(s)) ||
                        (x.mobile != null && x.mobile.Contains(s)));
                }

                var studentUsers = await query.OrderByDescending(s => s.Id).ToListAsync();
                var userIds = studentUsers.Select(u => u.Id).ToList();

                var profiles = await _context.StudentProfiles.AsNoTracking()
                    .Where(p => userIds.Contains(p.UserId))
                    .ToListAsync();

                var schoolIds = studentUsers.Where(u => u.Id_School.HasValue).Select(u => u.Id_School!.Value)
                    .Concat(profiles.Where(p => p.SchoolId.HasValue).Select(p => p.SchoolId!.Value))
                    .Distinct().ToList();
                var schools = await _context.Schools.AsNoTracking()
                    .Where(sc => schoolIds.Contains(sc.Id))
                    .ToDictionaryAsync(sc => sc.Id);

                var consultantIds = studentUsers.Where(u => u.Id_Moshaver.HasValue).Select(u => u.Id_Moshaver!.Value).Distinct().ToList();
                var consultantUsers = await _context.User.AsNoTracking()
                    .Where(u => consultantIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id);

                var dtos = studentUsers.Select(u =>
                {
                    var profile = profiles.FirstOrDefault(p => p.UserId == u.Id);
                    var sid = profile?.SchoolId ?? u.Id_School;
                    string? schoolName = null;
                    if (sid.HasValue && schools.TryGetValue(sid.Value, out var sc))
                    {
                        schoolName = sc.Name;
                    }

                    var cid = profile?.ConsultantId ?? u.Id_Moshaver;
                    string consultantName = "بدون مشاور";
                    if (!string.IsNullOrWhiteSpace(u.Moshaver))
                    {
                        consultantName = u.Moshaver;
                    }
                    else if (cid.HasValue && consultantUsers.TryGetValue(cid.Value, out var cu))
                    {
                        consultantName = cu.fname ?? "بدون مشاور";
                    }

                    return new StudentDto
                    {
                        Id = profile?.Id ?? u.Id,
                        UserId = u.Id,
                        FullName = u.fname ?? "",
                        NationalCode = u.code_meli ?? "",
                        StudentCode = profile?.StudentCode,
                        PhoneNumber = u.mobile,
                        FatherName = profile?.FatherName,
                        GradeLevel = profile?.GradeLevel ?? "پایه دوازدهم",
                        FieldOfStudy = profile?.FieldOfStudy ?? "علوم تجربی",
                        SchoolId = sid,
                        SchoolName = schoolName,
                        ConsultantId = cid,
                        ConsultantName = consultantName,
                        ProfilePictureUrl = u.pic,
                        IsActive = u.active,
                        CreatedAt = profile?.CreatedAt ?? DateTime.Now
                    };
                }).ToList();

                return Ok(ApiResponse<List<StudentDto>>.Ok(dtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<StudentDto>>.Fail($"خطا در دریافت لیست دانش‌آموزان: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<StudentDto>>> GetById(int id)
        {
            var profile = await _context.StudentProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id || x.UserId == id);
            var userId = profile?.UserId ?? id;

            var u = await _context.User.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            if (u == null)
                return NotFound(ApiResponse<StudentDto>.Fail("دانش‌آموز یافت نشد."));

            if (_currentUserService.IsStudent && u.Id != _currentUserService.UserId) return Forbid();
            if (_currentUserService.IsConsultant && !_currentUserService.IsSystemAdmin && u.Id_Moshaver != _currentUserService.UserId) return Forbid();
            if (_currentUserService.IsSchoolManager && !_currentUserService.IsSystemAdmin && u.Id_School != _currentUserService.SchoolId) return Forbid();

            string? schoolName = null;
            var sid = profile?.SchoolId ?? u.Id_School;
            if (sid.HasValue)
            {
                var school = await _context.Schools.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sid.Value);
                schoolName = school?.Name;
            }

            string consultantName = "بدون مشاور";
            var cid = profile?.ConsultantId ?? u.Id_Moshaver;
            if (!string.IsNullOrWhiteSpace(u.Moshaver))
            {
                consultantName = u.Moshaver;
            }
            else if (cid.HasValue)
            {
                var cu = await _context.User.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cid.Value);
                consultantName = cu?.fname ?? "بدون مشاور";
            }

            var dto = new StudentDto
            {
                Id = profile?.Id ?? u.Id,
                UserId = u.Id,
                FullName = u.fname ?? "",
                NationalCode = u.code_meli ?? "",
                StudentCode = profile?.StudentCode,
                PhoneNumber = u.mobile,
                FatherName = profile?.FatherName,
                GradeLevel = profile?.GradeLevel ?? "پایه دوازدهم",
                FieldOfStudy = profile?.FieldOfStudy ?? "علوم تجربی",
                SchoolId = sid,
                SchoolName = schoolName,
                ConsultantId = cid,
                ConsultantName = consultantName,
                ProfilePictureUrl = u.pic,
                IsActive = u.active,
                CreatedAt = profile?.CreatedAt ?? DateTime.Now
            };

            return Ok(ApiResponse<StudentDto>.Ok(dto));
        }

        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.SchoolManager}")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<StudentDto>>> Create([FromBody] CreateStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<StudentDto>.Fail(string.Join(" | ", errors)));
            }

            var trimmedNationalCode = dto.NationalCode.Trim();
            var existingUser = await _context.User.FirstOrDefaultAsync(u => u.code_meli == trimmedNationalCode);
            if (existingUser != null)
                return BadRequest(ApiResponse<StudentDto>.Fail("کاربری با این کد ملی قبلاً در سامانه ثبت شده است."));

            int? targetSchoolId = dto.SchoolId;
            if (_currentUserService.IsSchoolManager && !_currentUserService.IsSystemAdmin)
                targetSchoolId = _currentUserService.SchoolId;

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

                    var consProfile = await _context.ConsultantProfiles.FirstOrDefaultAsync(p => p.UserId == cons.Id || p.Id == dto.ConsultantId.Value);
                    if (consProfile == null)
                    {
                        consProfile = new ConsultantProfile
                        {
                            UserId = cons.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.ConsultantProfiles.Add(consProfile);
                        await _context.SaveChangesAsync();
                    }
                    consultantProfileId = consProfile.Id;
                }
            }

            var user = new User
            {
                fname = dto.FullName.Trim(),
                code_meli = trimmedNationalCode,
                pass = PasswordHelper.HashPassword(dto.Password),
                mobile = dto.PhoneNumber?.Trim() ?? "",
                gender = dto.Gender ?? "مرد",
                Rool = UserRoles.Student,
                Id_School = targetSchoolId,
                Id_Moshaver = consultantUserId,
                Moshaver = consultantName,
                active = true
            };

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            var profile = new StudentProfile
            {
                UserId = user.Id,
                NationalCode = trimmedNationalCode,
                StudentCode = dto.StudentCode,
                FatherName = dto.FatherName?.Trim(),
                GradeLevel = dto.GradeLevel?.Trim() ?? "پایه دوازدهم",
                FieldOfStudy = dto.FieldOfStudy?.Trim() ?? "علوم تجربی",
                SchoolId = targetSchoolId,
                ConsultantId = consultantProfileId,
                IsActive = true
            };

            _context.StudentProfiles.Add(profile);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("CreateStudent", "User", user.Id.ToString(), $"ثبت دانش‌آموز: {user.fname}");

            return Ok(ApiResponse<StudentDto>.Ok(new StudentDto
            {
                Id = profile.Id,
                UserId = user.Id,
                FullName = user.fname ?? "",
                NationalCode = user.code_meli ?? "",
                StudentCode = profile.StudentCode,
                PhoneNumber = user.mobile,
                FatherName = profile.FatherName,
                GradeLevel = profile.GradeLevel,
                FieldOfStudy = profile.FieldOfStudy,
                SchoolId = profile.SchoolId,
                ConsultantId = consultantUserId,
                ConsultantName = consultantName,
                IsActive = true,
                CreatedAt = profile.CreatedAt
            }, "دانش‌آموز با موفقیت ثبت شد."));
        }

        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.SchoolManager},{UserRoles.Consultant}")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Update(int id, [FromBody] UpdateStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<bool>.Fail(string.Join(" | ", errors)));
            }

            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.Id == id || p.UserId == id);
            var userId = profile?.UserId ?? id;

            var user = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound(ApiResponse<bool>.Fail("کاربر دانش‌آموز یافت نشد."));

            if (_currentUserService.IsSchoolManager && !_currentUserService.IsSystemAdmin && user.Id_School != _currentUserService.SchoolId)
                return Forbid();
            if (_currentUserService.IsConsultant && !_currentUserService.IsSystemAdmin && user.Id_Moshaver != _currentUserService.UserId)
                return Forbid();

            user.fname = dto.FullName.Trim();
            user.mobile = dto.PhoneNumber?.Trim() ?? user.mobile;
            if (!string.IsNullOrWhiteSpace(dto.Gender)) user.gender = dto.Gender;

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

                    var consProfile = await _context.ConsultantProfiles.FirstOrDefaultAsync(p => p.UserId == cons.Id || p.Id == dto.ConsultantId.Value);
                    if (consProfile == null)
                    {
                        consProfile = new ConsultantProfile
                        {
                            UserId = cons.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.ConsultantProfiles.Add(consProfile);
                        await _context.SaveChangesAsync();
                    }
                    consultantProfileId = consProfile.Id;
                }
            }

            if (_currentUserService.IsSystemAdmin)
            {
                user.active = dto.IsActive;
                user.Id_School = dto.SchoolId;
                user.Id_Moshaver = consultantUserId;
                user.Moshaver = consultantName;
            }
            else if (_currentUserService.IsSchoolManager)
            {
                user.Id_Moshaver = consultantUserId;
                user.Moshaver = consultantName;
            }

            if (profile == null)
            {
                profile = new StudentProfile
                {
                    UserId = user.Id,
                    NationalCode = user.code_meli ?? "",
                    IsActive = user.active
                };
                _context.StudentProfiles.Add(profile);
            }

            profile.StudentCode = dto.StudentCode;
            profile.FatherName = dto.FatherName?.Trim();
            profile.GradeLevel = dto.GradeLevel?.Trim();
            profile.FieldOfStudy = dto.FieldOfStudy?.Trim();
            profile.SchoolId = user.Id_School;
            profile.ConsultantId = consultantProfileId;
            profile.IsActive = user.active;
            profile.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.NewPassword) && _currentUserService.IsSystemAdmin)
            {
                user.pass = PasswordHelper.HashPassword(dto.NewPassword);
            }

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("UpdateStudent", "User", user.Id.ToString(), $"ویرایش دانش‌آموز: {user.fname}");

            return Ok(ApiResponse<bool>.Ok(true, "اطلاعات دانش‌آموز با موفقیت به‌روزرسانی شد."));
        }

        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.SchoolManager}")]
        [HttpPut("{id}/assign")]
        public async Task<ActionResult<ApiResponse<bool>>> Assign(int id, [FromBody] AssignStudentDto dto)
        {
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.Id == id || p.UserId == id);
            var userId = profile?.UserId ?? id;

            var user = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound(ApiResponse<bool>.Fail("دانش‌آموز یافت نشد."));

            if (_currentUserService.IsSchoolManager && !_currentUserService.IsSystemAdmin && user.Id_School != _currentUserService.SchoolId)
                return Forbid();

            if (_currentUserService.IsSystemAdmin && dto.SchoolId.HasValue)
                user.Id_School = dto.SchoolId.Value;

            int? consultantProfileId = null;
            if (dto.ConsultantId.HasValue && dto.ConsultantId.Value > 0)
            {
                var cons = await _context.User.FirstOrDefaultAsync(c => c.Id == dto.ConsultantId.Value);
                if (cons != null)
                {
                    user.Id_Moshaver = cons.Id;
                    user.Moshaver = cons.fname ?? "بدون مشاور";

                    var consProfile = await _context.ConsultantProfiles.FirstOrDefaultAsync(p => p.UserId == cons.Id || p.Id == dto.ConsultantId.Value);
                    if (consProfile == null)
                    {
                        consProfile = new ConsultantProfile
                        {
                            UserId = cons.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.ConsultantProfiles.Add(consProfile);
                        await _context.SaveChangesAsync();
                    }
                    consultantProfileId = consProfile.Id;
                }
            }
            else
            {
                user.Id_Moshaver = null;
                user.Moshaver = "بدون مشاور";
            }

            if (profile != null)
            {
                profile.SchoolId = user.Id_School;
                profile.ConsultantId = consultantProfileId;
                profile.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("AssignStudent", "User", user.Id.ToString(), $"انتساب دانش‌آموز: {user.fname}");

            return Ok(ApiResponse<bool>.Ok(true, "انتساب با موفقیت انجام شد."));
        }

        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.SchoolManager}")]
        [HttpPost("bulk-assign")]
        public async Task<ActionResult<ApiResponse<bool>>> BulkAssign([FromBody] BulkAssignStudentsDto dto)
        {
            if (dto.StudentIds == null || !dto.StudentIds.Any())
                return BadRequest(ApiResponse<bool>.Fail("هیچ دانش‌آموزی انتخاب نشده است."));

            var studentUsers = await _context.User
                .Where(u => dto.StudentIds.Contains(u.Id))
                .ToListAsync();

            if (_currentUserService.IsSchoolManager && !_currentUserService.IsSystemAdmin)
            {
                var schId = _currentUserService.SchoolId;
                studentUsers = studentUsers.Where(u => u.Id_School == schId).ToList();
            }

            var userIds = studentUsers.Select(u => u.Id).ToList();
            var profiles = await _context.StudentProfiles
                .Where(p => userIds.Contains(p.UserId))
                .ToListAsync();

            int? consultantProfileId = null;
            string consultantName = "بدون مشاور";
            int? consultantUserId = null;

            if (dto.UpdateConsultant && dto.ConsultantId.HasValue && dto.ConsultantId.Value > 0)
            {
                var cons = await _context.User.FirstOrDefaultAsync(c => c.Id == dto.ConsultantId.Value);
                if (cons != null)
                {
                    consultantUserId = cons.Id;
                    consultantName = cons.fname ?? "بدون مشاور";

                    var consProfile = await _context.ConsultantProfiles.FirstOrDefaultAsync(p => p.UserId == cons.Id || p.Id == dto.ConsultantId.Value);
                    if (consProfile == null)
                    {
                        consProfile = new ConsultantProfile
                        {
                            UserId = cons.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.ConsultantProfiles.Add(consProfile);
                        await _context.SaveChangesAsync();
                    }
                    consultantProfileId = consProfile.Id;
                }
            }

            foreach (var user in studentUsers)
            {
                if (dto.UpdateSchool && _currentUserService.IsSystemAdmin)
                {
                    user.Id_School = dto.SchoolId;
                }

                if (dto.UpdateConsultant)
                {
                    user.Id_Moshaver = consultantUserId;
                    user.Moshaver = consultantName;
                }

                var profile = profiles.FirstOrDefault(p => p.UserId == user.Id);
                if (profile == null)
                {
                    profile = new StudentProfile
                    {
                        UserId = user.Id,
                        NationalCode = user.code_meli ?? "",
                        IsActive = user.active,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.StudentProfiles.Add(profile);
                    profiles.Add(profile);
                }

                if (dto.UpdateSchool && _currentUserService.IsSystemAdmin)
                {
                    profile.SchoolId = dto.SchoolId;
                }

                if (dto.UpdateConsultant)
                {
                    profile.ConsultantId = consultantProfileId;
                }

                profile.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("BulkAssignStudents", "User", $"{studentUsers.Count} students", $"انتساب گروهی {studentUsers.Count} دانش‌آموز");

            return Ok(ApiResponse<bool>.Ok(true, $"انتساب گروهی {studentUsers.Count} دانش‌آموز با موفقیت ثبت گردید."));
        }

        [Authorize(Roles = UserRoles.SystemAdmin)]
        [HttpPost("bulk-delete")]
        public async Task<ActionResult<ApiResponse<bool>>> BulkDelete([FromBody] BulkDeleteStudentsDto dto)
        {
            if (dto.StudentIds == null || !dto.StudentIds.Any())
                return BadRequest(ApiResponse<bool>.Fail("هیچ رکوردی برای حذف انتخاب نشده است."));

            var profiles = await _context.StudentProfiles.Where(s => dto.StudentIds.Contains(s.Id) || dto.StudentIds.Contains(s.UserId)).ToListAsync();
            var targetUserIds = dto.StudentIds.Concat(profiles.Select(p => p.UserId)).Distinct().ToList();

            var users = await _context.User.Where(u => targetUserIds.Contains(u.Id)).ToListAsync();

            _context.StudentProfiles.RemoveRange(profiles);
            _context.User.RemoveRange(users);

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("BulkDeleteStudents", "User", $"{users.Count} students", $"حذف دسته‌جمعی {users.Count} دانش‌آموز");

            return Ok(ApiResponse<bool>.Ok(true, $"{users.Count} دانش‌آموز با موفقیت حذف شدند."));
        }

        [Authorize(Roles = UserRoles.SystemAdmin)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.Id == id || s.UserId == id);
            var userId = profile?.UserId ?? id;

            var user = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);

            if (profile != null) _context.StudentProfiles.Remove(profile);
            if (user != null) _context.User.Remove(user);

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("DeleteStudent", "User", id.ToString(), $"حذف دانش‌آموز");

            return Ok(ApiResponse<bool>.Ok(true, "دانش‌آموز با موفقیت حذف شد."));
        }
    }
}
