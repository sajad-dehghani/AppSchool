using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NovinApp.Server.Models;
using NovinApp.Server.MyContext;
using NovinApp.Shared.DTOs.Import;
using NovinApp.Shared.Entities;
using NovinApp.Shared.Enums;

namespace NovinApp.Server.Services
{
    public interface IExcelImportService
    {
        /// <summary>
        /// ایمپورت لیست دانش‌آموزان از فایل اکسل
        /// ستون‌های مورد انتظار: نام و نام خانوادگی | کد ملی | نام پدر | تلفن | پایه تحصیلی | رشته تحصیلی | شماره دانش‌آموزی
        /// </summary>
        Task<ImportResultDto> ImportStudentsAsync(Stream fileStream, string fileName, int uploadedByUserId, int? schoolId, int? consultantId, DuplicateHandlingMode duplicateMode);

        /// <summary>
        /// ایمپورت نتایج آزمون دانش‌آموزان از فایل اکسل
        /// ستون‌های مورد انتظار: کد ملی | درصد کل | تراز کل | ... (ستون‌های درسی داینامیک)
        /// </summary>
        Task<ImportResultDto> ImportExamResultsAsync(Stream fileStream, string fileName, int uploadedByUserId, int examId, DuplicateHandlingMode duplicateMode);

        /// <summary>
        /// دریافت تاریخچه ایمپورت‌ها
        /// </summary>
        Task<List<ImportSessionListDto>> GetImportHistoryAsync(int? uploadedByUserId = null);

        /// <summary>
        /// دریافت جزئیات یک ایمپورت
        /// </summary>
        Task<ImportResultDto?> GetImportDetailAsync(int importSessionId);
    }

    public class ExcelImportService : IExcelImportService
    {
        private readonly MyAppContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        // نگاشت نام ستون فارسی به فیلد
        private static readonly Dictionary<string, string> StudentColumnMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "نام و نام خانوادگی", "FullName" },
            { "نام", "FullName" },
            { "نام خانوادگی", "FullName" },
            { "کد ملی", "NationalCode" },
            { "کدملی", "NationalCode" },
            { "نام پدر", "FatherName" },
            { "تلفن", "PhoneNumber" },
            { "تلفن همراه", "PhoneNumber" },
            { "موبایل", "PhoneNumber" },
            { "پایه", "GradeLevel" },
            { "پایه تحصیلی", "GradeLevel" },
            { "رشته", "FieldOfStudy" },
            { "رشته تحصیلی", "FieldOfStudy" },
            { "شماره دانش‌آموزی", "StudentCode" },
            { "شماره دانش آموزی", "StudentCode" },
            { "کد دانش‌آموزی", "StudentCode" },
        };

        public ExcelImportService(MyAppContext context, UserManager<ApplicationUser> userManager, IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
        }

        #region Import Students

        public async Task<ImportResultDto> ImportStudentsAsync(Stream fileStream, string fileName, int uploadedByUserId, int? schoolId, int? consultantId, DuplicateHandlingMode duplicateMode)
        {
            var session = new ImportSession
            {
                FileName = fileName,
                UploadedByUserId = uploadedByUserId,
                Status = ImportStatus.Processing
            };
            _context.ImportSessions.Add(session);
            await _context.SaveChangesAsync();

            var errors = new List<ImportError>();
            int successCount = 0;
            int warningCount = 0;
            int totalRows = 0;

            try
            {
                using var workbook = new XLWorkbook(fileStream);
                var worksheet = workbook.Worksheets.First();
                var headerRow = worksheet.Row(1);

                // نگاشت شماره ستون به فیلد
                var columnMapping = MapColumns(headerRow);

                if (!columnMapping.ContainsValue("NationalCode"))
                {
                    session.Status = ImportStatus.Failed;
                    session.SummaryMessage = "ستون «کد ملی» در فایل یافت نشد. این ستون الزامی است.";
                    await _context.SaveChangesAsync();
                    return ToResultDto(session, errors);
                }

                var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
                totalRows = lastRow - 1; // minus header

                // دریافت تمام کدملی‌های موجود در دیتابیس برای سرعت
                var existingNationalCodes = await _context.Set<StudentProfile>()
                    .Select(s => s.NationalCode)
                    .ToListAsync();
                var existingNcSet = new HashSet<string>(existingNationalCodes);

                var existingUsernames = await _context.Users
                    .Select(u => u.UserName!)
                    .ToListAsync();
                var existingUsernameSet = new HashSet<string>(existingUsernames, StringComparer.OrdinalIgnoreCase);

                for (int rowNum = 2; rowNum <= lastRow; rowNum++)
                {
                    var row = worksheet.Row(rowNum);

                    try
                    {
                        var rowData = ExtractStudentRow(row, columnMapping, rowNum, errors);
                        if (rowData == null) continue; // خطا ثبت شده

                        // بررسی تکراری بودن
                        if (existingNcSet.Contains(rowData.NationalCode))
                        {
                            switch (duplicateMode)
                            {
                                case DuplicateHandlingMode.Skip:
                                    warningCount++;
                                    errors.Add(new ImportError
                                    {
                                        ImportSessionId = session.Id,
                                        RowNumber = rowNum,
                                        ColumnName = "کد ملی",
                                        RawValue = rowData.NationalCode,
                                        ErrorType = "Duplicate",
                                        ErrorMessage = $"دانش‌آموز با کد ملی {rowData.NationalCode} قبلاً ثبت شده. رد شد."
                                    });
                                    continue;

                                case DuplicateHandlingMode.Update:
                                    await UpdateExistingStudent(rowData, schoolId, consultantId);
                                    successCount++;
                                    warningCount++;
                                    continue;

                                case DuplicateHandlingMode.Error:
                                    errors.Add(new ImportError
                                    {
                                        ImportSessionId = session.Id,
                                        RowNumber = rowNum,
                                        ColumnName = "کد ملی",
                                        RawValue = rowData.NationalCode,
                                        ErrorType = "DuplicateError",
                                        ErrorMessage = $"دانش‌آموز با کد ملی {rowData.NationalCode} تکراری است."
                                    });
                                    continue;
                            }
                        }

                        // ساخت کاربر جدید
                        string username = rowData.NationalCode;
                        if (existingUsernameSet.Contains(username))
                        {
                            username = $"s{rowData.NationalCode}";
                        }

                        var user = new ApplicationUser
                        {
                            UserName = username,
                            FullName = rowData.FullName,
                            NationalCode = rowData.NationalCode,
                            PhoneNumber = rowData.PhoneNumber,
                            Gender = "نامشخص"
                        };

                        var createResult = await _userManager.CreateAsync(user, $"Novin@{rowData.NationalCode}");
                        if (!createResult.Succeeded)
                        {
                            errors.Add(new ImportError
                            {
                                ImportSessionId = session.Id,
                                RowNumber = rowNum,
                                ErrorType = "CreateUserFailed",
                                ErrorMessage = string.Join(" | ", createResult.Errors.Select(e => e.Description))
                            });
                            continue;
                        }

                        await _userManager.AddToRoleAsync(user, "Student");

                        var studentProfile = new StudentProfile
                        {
                            UserId = user.Id,
                            NationalCode = rowData.NationalCode,
                            FatherName = rowData.FatherName,
                            GradeLevel = rowData.GradeLevel ?? "پایه دوازدهم",
                            FieldOfStudy = rowData.FieldOfStudy ?? "علوم تجربی",
                            StudentCode = rowData.StudentCode,
                            SchoolId = schoolId,
                            ConsultantId = consultantId,
                            IsActive = true
                        };

                        _context.Set<StudentProfile>().Add(studentProfile);
                        existingNcSet.Add(rowData.NationalCode);
                        existingUsernameSet.Add(username);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new ImportError
                        {
                            ImportSessionId = session.Id,
                            RowNumber = rowNum,
                            ErrorType = "UnexpectedError",
                            ErrorMessage = ex.Message
                        });
                    }
                }

                await _context.SaveChangesAsync();

                // ذخیره خطاها
                if (errors.Any())
                {
                    _context.Set<ImportError>().AddRange(errors);
                    await _context.SaveChangesAsync();
                }

                session.TotalRows = totalRows;
                session.SuccessfulRows = successCount;
                session.FailedRows = errors.Count(e => e.ErrorType != "Duplicate");
                session.WarningRows = warningCount;
                session.Status = errors.Any(e => e.ErrorType != "Duplicate")
                    ? ImportStatus.PartialFailure
                    : ImportStatus.Completed;
                session.SummaryMessage = $"از {totalRows} ردیف، {successCount} مورد با موفقیت ثبت شد. {session.FailedRows} خطا، {warningCount} هشدار.";
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("ImportStudents", "ImportSession", session.Id.ToString(),
                    $"ایمپورت {successCount}/{totalRows} دانش‌آموز از فایل {fileName}");

                return ToResultDto(session, errors);
            }
            catch (Exception ex)
            {
                session.Status = ImportStatus.Failed;
                session.TotalRows = totalRows;
                session.SummaryMessage = $"خطای سیستمی در پردازش فایل: {ex.Message}";
                await _context.SaveChangesAsync();
                return ToResultDto(session, errors);
            }
        }

        #endregion

        #region Import Exam Results

        public async Task<ImportResultDto> ImportExamResultsAsync(Stream fileStream, string fileName, int uploadedByUserId, int examId, DuplicateHandlingMode duplicateMode)
        {
            var exam = await _context.Set<Exam>().FindAsync(examId);
            if (exam == null)
            {
                return new ImportResultDto
                {
                    Status = ImportStatus.Failed,
                    SummaryMessage = "آزمون انتخاب‌شده یافت نشد."
                };
            }

            var session = new ImportSession
            {
                FileName = fileName,
                UploadedByUserId = uploadedByUserId,
                ExamId = examId,
                Status = ImportStatus.Processing
            };
            _context.ImportSessions.Add(session);
            await _context.SaveChangesAsync();

            var errors = new List<ImportError>();
            int successCount = 0;
            int warningCount = 0;
            int totalRows = 0;

            try
            {
                using var workbook = new XLWorkbook(fileStream);
                var worksheet = workbook.Worksheets.First();
                var headerRow = worksheet.Row(1);

                // ستون‌های ثابت: کد ملی، درصد کل، تراز کل، ...
                // ستون‌های داینامیک درسی: هر ستون دیگر = نام درس
                int? ncColIndex = null;
                int? totalPercentColIndex = null;
                int? totalTarazColIndex = null;
                var subjectColumns = new Dictionary<int, string>(); // colIndex -> subjectName

                var lastColUsed = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;

                for (int col = 1; col <= lastColUsed; col++)
                {
                    var headerText = headerRow.Cell(col).GetString().Trim();
                    if (string.IsNullOrEmpty(headerText)) continue;

                    if (headerText.Contains("کد ملی") || headerText.Contains("کدملی"))
                        ncColIndex = col;
                    else if (headerText.Contains("درصد کل") || headerText.Contains("درصدکل"))
                        totalPercentColIndex = col;
                    else if (headerText.Contains("تراز کل") || headerText.Contains("ترازکل") || headerText.Contains("تراز"))
                        totalTarazColIndex = col;
                    else
                        subjectColumns[col] = headerText; // ستون درسی داینامیک
                }

                if (ncColIndex == null)
                {
                    session.Status = ImportStatus.Failed;
                    session.SummaryMessage = "ستون «کد ملی» در فایل نتایج یافت نشد.";
                    await _context.SaveChangesAsync();
                    return ToResultDto(session, errors);
                }

                // لود دروس موجود
                var subjects = await _context.Set<Subject>().ToListAsync();

                var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
                totalRows = lastRow - 1;

                for (int rowNum = 2; rowNum <= lastRow; rowNum++)
                {
                    var row = worksheet.Row(rowNum);
                    try
                    {
                        var nationalCode = row.Cell(ncColIndex.Value).GetString().Trim();
                        if (string.IsNullOrWhiteSpace(nationalCode))
                        {
                            errors.Add(new ImportError
                            {
                                ImportSessionId = session.Id,
                                RowNumber = rowNum,
                                ColumnName = "کد ملی",
                                ErrorType = "EmptyField",
                                ErrorMessage = "کد ملی خالی است."
                            });
                            continue;
                        }

                        var student = await _context.Set<StudentProfile>()
                            .FirstOrDefaultAsync(s => s.NationalCode == nationalCode);

                        if (student == null)
                        {
                            errors.Add(new ImportError
                            {
                                ImportSessionId = session.Id,
                                RowNumber = rowNum,
                                ColumnName = "کد ملی",
                                RawValue = nationalCode,
                                ErrorType = "StudentNotFound",
                                ErrorMessage = $"دانش‌آموزی با کد ملی {nationalCode} در سامانه یافت نشد."
                            });
                            continue;
                        }

                        // بررسی تکراری بودن نتیجه
                        var existingResult = await _context.Set<StudentExamResult>()
                            .FirstOrDefaultAsync(r => r.StudentProfileId == student.Id && r.ExamId == examId);

                        if (existingResult != null)
                        {
                            if (duplicateMode == DuplicateHandlingMode.Skip)
                            {
                                warningCount++;
                                continue;
                            }
                            else if (duplicateMode == DuplicateHandlingMode.Error)
                            {
                                errors.Add(new ImportError
                                {
                                    ImportSessionId = session.Id,
                                    RowNumber = rowNum,
                                    ErrorType = "DuplicateResult",
                                    ErrorMessage = $"نتیجه آزمون برای {nationalCode} قبلاً ثبت شده."
                                });
                                continue;
                            }
                            // Update mode: حذف قبلی و ساخت مجدد
                            var oldSubjectResults = await _context.Set<StudentExamSubjectResult>()
                                .Where(sr => sr.StudentExamResultId == existingResult.Id)
                                .ToListAsync();
                            _context.Set<StudentExamSubjectResult>().RemoveRange(oldSubjectResults);
                            _context.Set<StudentExamResult>().Remove(existingResult);
                            await _context.SaveChangesAsync();
                        }

                        float totalPercent = totalPercentColIndex.HasValue
                            ? ParseFloat(row.Cell(totalPercentColIndex.Value).GetString())
                            : 0;
                        float totalTaraz = totalTarazColIndex.HasValue
                            ? ParseFloat(row.Cell(totalTarazColIndex.Value).GetString())
                            : 0;

                        var examResult = new StudentExamResult
                        {
                            StudentProfileId = student.Id,
                            ExamId = examId,
                            TotalPercent = totalPercent,
                            TotalTaraz = totalTaraz,
                            ImportSessionId = session.Id
                        };
                        _context.Set<StudentExamResult>().Add(examResult);
                        await _context.SaveChangesAsync();

                        // ثبت نتایج درسی
                        foreach (var (colIndex, subjectName) in subjectColumns)
                        {
                            var cellValue = row.Cell(colIndex).GetString().Trim();
                            if (string.IsNullOrWhiteSpace(cellValue)) continue;

                            float percent = ParseFloat(cellValue);
                            var subject = subjects.FirstOrDefault(s =>
                                s.Title.Contains(subjectName) || subjectName.Contains(s.Title));

                            var subjectResult = new StudentExamSubjectResult
                            {
                                StudentExamResultId = examResult.Id,
                                SubjectId = subject?.Id,
                                SubjectName = subjectName,
                                Percent = percent
                            };
                            _context.Set<StudentExamSubjectResult>().Add(subjectResult);
                        }

                        await _context.SaveChangesAsync();
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new ImportError
                        {
                            ImportSessionId = session.Id,
                            RowNumber = rowNum,
                            ErrorType = "UnexpectedError",
                            ErrorMessage = ex.Message
                        });
                    }
                }

                if (errors.Any())
                {
                    _context.Set<ImportError>().AddRange(errors);
                    await _context.SaveChangesAsync();
                }

                session.TotalRows = totalRows;
                session.SuccessfulRows = successCount;
                session.FailedRows = errors.Count;
                session.WarningRows = warningCount;
                session.Status = errors.Any() ? ImportStatus.PartialFailure : ImportStatus.Completed;
                session.SummaryMessage = $"از {totalRows} ردیف، {successCount} نتیجه با موفقیت ثبت شد.";
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("ImportExamResults", "ImportSession", session.Id.ToString(),
                    $"ایمپورت نتایج آزمون «{exam.Title}» - {successCount}/{totalRows}");

                return ToResultDto(session, errors);
            }
            catch (Exception ex)
            {
                session.Status = ImportStatus.Failed;
                session.TotalRows = totalRows;
                session.SummaryMessage = $"خطای سیستمی: {ex.Message}";
                await _context.SaveChangesAsync();
                return ToResultDto(session, errors);
            }
        }

        #endregion

        #region History & Detail

        public async Task<List<ImportSessionListDto>> GetImportHistoryAsync(int? uploadedByUserId = null)
        {
            var query = _context.ImportSessions.AsQueryable();
            if (uploadedByUserId.HasValue)
                query = query.Where(s => s.UploadedByUserId == uploadedByUserId.Value);

            return await query
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new ImportSessionListDto
                {
                    Id = s.Id,
                    FileName = s.FileName,
                    Status = s.Status,
                    TotalRows = s.TotalRows,
                    SuccessfulRows = s.SuccessfulRows,
                    FailedRows = s.FailedRows,
                    SummaryMessage = s.SummaryMessage,
                    CreatedAt = s.CreatedAt,
                    ExamTitle = s.Exam != null ? s.Exam.Title : null
                })
                .Take(50)
                .ToListAsync();
        }

        public async Task<ImportResultDto?> GetImportDetailAsync(int importSessionId)
        {
            var session = await _context.ImportSessions
                .Include(s => s.Errors)
                .FirstOrDefaultAsync(s => s.Id == importSessionId);

            if (session == null) return null;
            return ToResultDto(session, session.Errors.ToList());
        }

        #endregion

        #region Helpers

        private Dictionary<int, string> MapColumns(IXLRow headerRow)
        {
            var mapping = new Dictionary<int, string>();
            var lastCol = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;

            for (int col = 1; col <= lastCol; col++)
            {
                var headerText = headerRow.Cell(col).GetString().Trim();
                if (string.IsNullOrEmpty(headerText)) continue;

                if (StudentColumnMap.TryGetValue(headerText, out var fieldName))
                {
                    mapping[col] = fieldName;
                }
            }

            return mapping;
        }

        private StudentRowData? ExtractStudentRow(IXLRow row, Dictionary<int, string> columnMapping, int rowNum, List<ImportError> errors)
        {
            var data = new StudentRowData();

            foreach (var (colIndex, fieldName) in columnMapping)
            {
                var cellValue = row.Cell(colIndex).GetString().Trim();

                switch (fieldName)
                {
                    case "FullName":
                        data.FullName = cellValue;
                        break;
                    case "NationalCode":
                        data.NationalCode = cellValue;
                        break;
                    case "FatherName":
                        data.FatherName = cellValue;
                        break;
                    case "PhoneNumber":
                        data.PhoneNumber = cellValue;
                        break;
                    case "GradeLevel":
                        data.GradeLevel = cellValue;
                        break;
                    case "FieldOfStudy":
                        data.FieldOfStudy = cellValue;
                        break;
                    case "StudentCode":
                        if (long.TryParse(cellValue, out var code))
                            data.StudentCode = code;
                        break;
                }
            }

            // اعتبارسنجی
            if (string.IsNullOrWhiteSpace(data.NationalCode))
            {
                errors.Add(new ImportError
                {
                    RowNumber = rowNum,
                    ColumnName = "کد ملی",
                    ErrorType = "EmptyRequired",
                    ErrorMessage = "کد ملی خالی است."
                });
                return null;
            }

            if (data.NationalCode.Length != 10 || !data.NationalCode.All(char.IsDigit))
            {
                errors.Add(new ImportError
                {
                    RowNumber = rowNum,
                    ColumnName = "کد ملی",
                    RawValue = data.NationalCode,
                    ErrorType = "InvalidFormat",
                    ErrorMessage = "کد ملی باید دقیقاً ۱۰ رقم باشد."
                });
                return null;
            }

            if (string.IsNullOrWhiteSpace(data.FullName))
            {
                errors.Add(new ImportError
                {
                    RowNumber = rowNum,
                    ColumnName = "نام",
                    ErrorType = "EmptyRequired",
                    ErrorMessage = "نام و نام خانوادگی خالی است."
                });
                return null;
            }

            return data;
        }

        private async Task UpdateExistingStudent(StudentRowData data, int? schoolId, int? consultantId)
        {
            var student = await _context.Set<StudentProfile>()
                .FirstOrDefaultAsync(s => s.NationalCode == data.NationalCode);

            if (student == null) return;

            if (!string.IsNullOrWhiteSpace(data.FatherName)) student.FatherName = data.FatherName;
            if (!string.IsNullOrWhiteSpace(data.GradeLevel)) student.GradeLevel = data.GradeLevel;
            if (!string.IsNullOrWhiteSpace(data.FieldOfStudy)) student.FieldOfStudy = data.FieldOfStudy;
            if (data.StudentCode.HasValue) student.StudentCode = data.StudentCode;
            if (schoolId.HasValue) student.SchoolId = schoolId;
            if (consultantId.HasValue) student.ConsultantId = consultantId;
            student.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        private static float ParseFloat(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            value = value.Replace("٪", "").Replace("%", "").Trim();
            return float.TryParse(value, out var result) ? result : 0;
        }

        private static ImportResultDto ToResultDto(ImportSession session, List<ImportError> errors)
        {
            return new ImportResultDto
            {
                ImportSessionId = session.Id,
                FileName = session.FileName,
                Status = session.Status,
                TotalRows = session.TotalRows,
                SuccessfulRows = session.SuccessfulRows,
                FailedRows = session.FailedRows,
                WarningRows = session.WarningRows,
                SummaryMessage = session.SummaryMessage,
                CreatedAt = session.CreatedAt,
                Errors = errors.Select(e => new ImportErrorDto
                {
                    RowNumber = e.RowNumber,
                    ColumnName = e.ColumnName,
                    RawValue = e.RawValue,
                    ErrorType = e.ErrorType,
                    ErrorMessage = e.ErrorMessage
                }).ToList()
            };
        }

        private class StudentRowData
        {
            public string FullName { get; set; } = string.Empty;
            public string NationalCode { get; set; } = string.Empty;
            public string? FatherName { get; set; }
            public string? PhoneNumber { get; set; }
            public string? GradeLevel { get; set; }
            public string? FieldOfStudy { get; set; }
            public long? StudentCode { get; set; }
        }

        #endregion
    }
}
