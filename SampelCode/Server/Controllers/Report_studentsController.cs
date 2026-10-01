using EFCore.BulkExtensions;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovinApp.Server.MyContext;
using NovinApp.Shared;
using System;
using static System.Collections.Specialized.BitVector32;

namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Report_studentsController : ControllerBase
    {
        private readonly MyAppContext _context;
        public Report_studentsController(MyAppContext context)
        {
            _context = context;
        }

        [HttpGet("all/{code_m}")]
        public async Task<IActionResult> Get_all(string code_m)
        {
            if (string.IsNullOrWhiteSpace(code_m))
                return BadRequest("کد ملی ارسال نشده است.");

            string cleanCode = code_m.Trim();
            var devs = await _context.Report_students
                .Where(x => x.Student_NationalCode != null && (x.Student_NationalCode == cleanCode || x.Student_NationalCode.Trim() == cleanCode))
                .OrderBy(x => x.ID)
                .ToListAsync();

            return Ok(devs);
        }


        [HttpGet("login/{id}")]
        public async Task<IActionResult> Get(int id)
        {

            var dev = await _context.Report_students.FirstAsync(d => d.ID == id);
            if (dev != null)
                return Ok(dev);
            else
                return NotFound();

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, bool mode)
        {

            var dev = await _context.Report_students.FirstAsync(d => d.ID == id);
            if (dev != null)
                return Ok(dev);
            else
                return NotFound();

        }

        [HttpGet("CodeMeli/{code}")]
        public async Task<IActionResult> Get_code_meli(string code)
        {

            var dev = await _context.Report_students
                .Where(x => x.Student_NationalCode == code)
                .ToListAsync();
            if (dev != null && dev.Count > 0)
                return Ok(dev);
            else
                return NotFound();

        }
        [HttpPost("upload-excel")]
        public async Task<IActionResult> UploadExcel([FromBody] List<Report_students> students)
        {
            if (students == null || students.Count == 0)
                return BadRequest("No data received");

            await _context.BulkInsertAsync(students);

            return Ok("Data inserted successfully!");
        }

        [HttpPost("upload-excel-stream")]
        [RequestSizeLimit(524_288_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 524_288_000)]
        public async Task<IActionResult> UploadExcelStream(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ExcelBulkUploadResult
                {
                    Success = false,
                    ErrorMessage = "هیچ فایلی برای بارگذاری ارسال نشده است."
                });
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            string tempFilePath = Path.GetTempFileName();
            int totalInserted = 0;

            try
            {
                // ذخیره سریع فایل روی دیسک موقت جهت جلوگیری از پر شدن حافظه RAM سرور
                await using (var stream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                {
                    await file.CopyToAsync(stream);
                }

                // فعال‌سازی پشتیبانی کاراکترهای فارسی و قدیمی
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                await using var xlStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, false);
                using var workbook = new XLWorkbook(xlStream);
                var worksheet = workbook.Worksheets.First();

                var lastRowUsed = worksheet.LastRowUsed()?.RowNumber() ?? 1;
                if (lastRowUsed <= 1)
                {
                    return Ok(new ExcelBulkUploadResult
                    {
                        Success = true,
                        TotalRows = 0,
                        ElapsedSeconds = 0,
                        RowsPerSecond = 0,
                        Message = "فایل ارسالی خالی است."
                    });
                }

                // آماده‌سازی جدول موقت و اتصال سریع مستقیم به دیتابیس
                var dataTable = BuildReportStudentsDataTable();
                var connStr = _context.Database.GetConnectionString();

                if (string.IsNullOrWhiteSpace(connStr))
                {
                    throw new InvalidOperationException("رشته اتصال به پایگاه داده یافت نشد.");
                }

                using var sqlConn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
                await sqlConn.OpenAsync();

                using var bulkCopy = new Microsoft.Data.SqlClient.SqlBulkCopy(
                    sqlConn,
                    Microsoft.Data.SqlClient.SqlBulkCopyOptions.TableLock | Microsoft.Data.SqlClient.SqlBulkCopyOptions.UseInternalTransaction,
                    null)
                {
                    DestinationTableName = "Report_students",
                    BatchSize = 25000,
                    BulkCopyTimeout = 0 // بدون محدودیت زمانی برای حجم‌های میلیونی
                };

                // مپینگ دقیق ستون‌ها به نام ستون‌های جدول دیتابیس (بدون ستون شناسه ID)
                foreach (System.Data.DataColumn col in dataTable.Columns)
                {
                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                }

                var lastColUsed = worksheet.LastColumnUsed()?.ColumnNumber() ?? 1;
                int fieldCount = Math.Max(lastColUsed, 265);
                object[] buffer = new object[fieldCount];
                var now = DateTime.Now;
                string batchDesc = string.IsNullOrWhiteSpace(file.FileName) ? $"آپلود_{now:yyyy/MM/dd_HH:mm:ss}" : file.FileName;
                const int batchThreshold = 25000;

                // شروع از ردیف ۲ (رد کردن سرستون)
                for (int rowNum = 2; rowNum <= lastRowUsed; rowNum++)
                {
                    var row = worksheet.Row(rowNum);
                    Array.Clear(buffer, 0, buffer.Length);

                    for (int col = 1; col <= lastColUsed; col++)
                    {
                        var cell = row.Cell(col);
                        if (!cell.IsEmpty())
                        {
                            buffer[col - 1] = cell.Value.Type switch
                            {
                                XLDataType.Number => cell.GetDouble(),
                                XLDataType.Text => cell.GetString(),
                                XLDataType.Boolean => cell.GetBoolean(),
                                XLDataType.DateTime => cell.GetDateTime(),
                                _ => cell.GetString()
                            };
                        }
                    }

                    long studentCode = SafeLong(buffer[240]);
                    string? fullName = SafeString(buffer[244]);
                    string? nationalCode = SafeString(buffer[246]);

                    // نادیده گرفتن رکوردهای کاملاً خالی انتهای فایل
                    if (studentCode == 0 && string.IsNullOrWhiteSpace(fullName) && string.IsNullOrWhiteSpace(nationalCode))
                    {
                        continue;
                    }

                    var dataRow = dataTable.NewRow();
                    PopulateDataRow(dataRow, buffer, now, batchDesc);
                    dataTable.Rows.Add(dataRow);
                    totalInserted++;

                    // درج دسته‌ای هر ۲۵ هزار رکورد با تخلیه سریع حافظه
                    if (dataTable.Rows.Count >= batchThreshold)
                    {
                        await bulkCopy.WriteToServerAsync(dataTable);
                        dataTable.Clear();
                    }
                }

                // درج باقیمانده سطرها در انتهای پردازش
                if (dataTable.Rows.Count > 0)
                {
                    await bulkCopy.WriteToServerAsync(dataTable);
                    dataTable.Clear();
                }

                stopwatch.Stop();
                double elapsed = Math.Max(0.01, stopwatch.Elapsed.TotalSeconds);
                double rps = totalInserted / elapsed;

                return Ok(new ExcelBulkUploadResult
                {
                    Success = true,
                    TotalRows = totalInserted,
                    ElapsedSeconds = Math.Round(elapsed, 2),
                    RowsPerSecond = Math.Round(rps, 0),
                    Message = $"ثبت فوق‌العاده سریع با موفقیت انجام شد: {totalInserted:N0} رکورد در مدت {elapsed:F2} ثانیه (سرعت: {rps:N0} رکورد بر ثانیه)"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ExcelBulkUploadResult
                {
                    Success = false,
                    TotalRows = totalInserted,
                    ErrorMessage = $"خطا در پردازش و بارگذاری داده‌ها: {ex.Message}"
                });
            }
            finally
            {
                // پاکسازی حتمی فایل موقت از روی دیسک
                try
                {
                    if (System.IO.File.Exists(tempFilePath))
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                }
                catch { }
            }
        }

        [HttpGet("upload-batches")]
        public async Task<IActionResult> GetUploadBatches()
        {
            var list = new List<UploadBatchSummary>();
            var connStr = _context.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connStr)) return Ok(list);

            try
            {
                using var sqlConn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
                await sqlConn.OpenAsync();

                string sql = @"
                    SELECT TOP 30
                        UploadTime,
                        COALESCE(MAX(UploadDescription), '') AS BatchId,
                        COUNT(*) AS RecordCount,
                        COALESCE(MAX(ExamName), '') AS ExamName,
                        MIN(ID) AS MinId,
                        MAX(ID) AS MaxId
                    FROM Report_students
                    GROUP BY UploadTime
                    ORDER BY MAX(ID) DESC";

                using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, sqlConn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var dt = (DateTime)reader["UploadTime"];
                    string bId = reader["BatchId"].ToString() ?? "";
                    list.Add(new UploadBatchSummary
                    {
                        UploadTime = dt,
                        BatchId = bId,
                        FileName = string.IsNullOrWhiteSpace(bId) ? $"آپلود مورخ {dt:yyyy/MM/dd HH:mm:ss}" : bId,
                        RecordCount = Convert.ToInt32(reader["RecordCount"]),
                        ExamName = reader["ExamName"].ToString() ?? "",
                        MinId = Convert.ToInt32(reader["MinId"]),
                        MaxId = Convert.ToInt32(reader["MaxId"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching batches: {ex.Message}");
            }

            return Ok(list);
        }

        [HttpDelete("delete-last-upload")]
        public async Task<IActionResult> DeleteLastUpload()
        {
            try
            {
                var connStr = _context.Database.GetConnectionString();
                if (string.IsNullOrWhiteSpace(connStr))
                    return BadRequest(new DeleteBatchResult { Success = false, ErrorMessage = "رشته اتصال به دیتابیس یافت نشد." });

                using var sqlConn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
                await sqlConn.OpenAsync();

                string findSql = @"
                    SELECT TOP 1 
                        UploadTime, 
                        COALESCE(MAX(UploadDescription), '') AS BatchId, 
                        COUNT(*) AS cnt, 
                        COALESCE(MAX(ExamName), '') AS Exam 
                    FROM Report_students 
                    GROUP BY UploadTime 
                    ORDER BY MAX(ID) DESC";

                DateTime latestTime;
                string batchDesc;
                string examName;

                using (var findCmd = new Microsoft.Data.SqlClient.SqlCommand(findSql, sqlConn))
                using (var reader = await findCmd.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        return Ok(new DeleteBatchResult
                        {
                            Success = false,
                            DeletedCount = 0,
                            Message = "هیچ داده‌ای در جدول کارنامه‌ها برای حذف یافت نشد."
                        });
                    }

                    latestTime = (DateTime)reader["UploadTime"];
                    batchDesc = reader["BatchId"].ToString() ?? "";
                    examName = reader["Exam"].ToString() ?? "";
                }

                // حذف یکجا و فوق‌العاده سریع تمام رکوردهای آخرین آپلود در سطح موتور SQL
                string deleteSql = "DELETE FROM Report_students WHERE UploadTime = @Time";
                using var deleteCmd = new Microsoft.Data.SqlClient.SqlCommand(deleteSql, sqlConn);
                deleteCmd.CommandTimeout = 0; // بدون محدودیت زمانی برای حجم‌های میلیونی
                deleteCmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@Time", System.Data.SqlDbType.DateTime2) { Value = latestTime });

                int deleted = await deleteCmd.ExecuteNonQueryAsync();

                string fileInfo = !string.IsNullOrWhiteSpace(batchDesc) ? $"فایل «{batchDesc}»" : $"آپلود مورخ {latestTime:yyyy/MM/dd HH:mm:ss}";
                if (!string.IsNullOrWhiteSpace(examName))
                {
                    fileInfo += $" ({examName})";
                }

                return Ok(new DeleteBatchResult
                {
                    Success = true,
                    DeletedCount = deleted,
                    Message = $"تمام {deleted:N0} رکورد مربوط به آخرین {fileInfo} با موفقیت و به صورت یکجا حذف شدند."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new DeleteBatchResult
                {
                    Success = false,
                    ErrorMessage = $"خطا در حذف آخرین دسته آپلود شده: {ex.Message}"
                });
            }
        }

        [HttpDelete("delete-batch")]
        public async Task<IActionResult> DeleteBatch([FromQuery] string uploadTimeStr)
        {
            try
            {
                if (!DateTime.TryParse(uploadTimeStr, out DateTime targetTime))
                {
                    return BadRequest(new DeleteBatchResult
                    {
                        Success = false,
                        ErrorMessage = "فرمت زمان آپلود معتبر نیست."
                    });
                }

                var connStr = _context.Database.GetConnectionString();
                using var sqlConn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
                await sqlConn.OpenAsync();

                string deleteSql = "DELETE FROM Report_students WHERE UploadTime = @Time";
                using var deleteCmd = new Microsoft.Data.SqlClient.SqlCommand(deleteSql, sqlConn);
                deleteCmd.CommandTimeout = 0;
                deleteCmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@Time", System.Data.SqlDbType.DateTime2) { Value = targetTime });

                int deleted = await deleteCmd.ExecuteNonQueryAsync();

                return Ok(new DeleteBatchResult
                {
                    Success = true,
                    DeletedCount = deleted,
                    Message = $"{deleted:N0} رکورد متعلق به دسته انتخابی ({targetTime:yyyy/MM/dd HH:mm:ss}) با موفقیت حذف شدند."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new DeleteBatchResult
                {
                    Success = false,
                    ErrorMessage = $"خطا در حذف دسته: {ex.Message}"
                });
            }
        }

        private static System.Data.DataTable BuildReportStudentsDataTable()
        {
            var dt = new System.Data.DataTable("Report_students");

            // ستون‌های ۱۴ درس
            for (int i = 1; i <= 14; i++)
            {
                dt.Columns.Add($"lesson{i}_Name", typeof(string));
                dt.Columns.Add($"lesson{i}_Percent", typeof(float));
                dt.Columns.Add($"lesson{i}_PercentAvarage", typeof(float));
                dt.Columns.Add($"lesson{i}_PercentMax", typeof(float));
                dt.Columns.Add($"lesson{i}_True", typeof(float));
                dt.Columns.Add($"lesson{i}_False", typeof(float));
                dt.Columns.Add($"lesson{i}_Blank", typeof(float));
                dt.Columns.Add($"lesson{i}_Taraz", typeof(float));
                dt.Columns.Add($"lesson{i}_MaxTararz", typeof(float));
                dt.Columns.Add($"lesson{i}_PercentMin", typeof(float));
                dt.Columns.Add($"lesson{i}_TarazMin", typeof(float));
                dt.Columns.Add($"lesson{i}_RankSchool", typeof(float));
                dt.Columns.Add($"lesson{i}_RankArea", typeof(float));
                dt.Columns.Add($"lesson{i}_RankSex", typeof(float));
                dt.Columns.Add($"lesson{i}_RankTotal", typeof(float));
                dt.Columns.Add($"lesson{i}_Status", typeof(string));
            }

            // ستون‌های کل (Total)
            dt.Columns.Add("Total_Name", typeof(string));
            dt.Columns.Add("Total_Percent", typeof(float));
            dt.Columns.Add("Total_PercentAvarage", typeof(float));
            dt.Columns.Add("Total_PercentMax", typeof(float));
            dt.Columns.Add("Total_True", typeof(float));
            dt.Columns.Add("Total_False", typeof(float));
            dt.Columns.Add("Total_Blank", typeof(float));
            dt.Columns.Add("Total_Taraz", typeof(float));
            dt.Columns.Add("Total_MaxTararz", typeof(float));
            dt.Columns.Add("Total_PercentMin", typeof(float));
            dt.Columns.Add("Total_TarazMin", typeof(float));
            dt.Columns.Add("Total_RankSchool", typeof(float));
            dt.Columns.Add("Total_RankArea", typeof(float));
            dt.Columns.Add("Total_RankSex", typeof(float));
            dt.Columns.Add("Total_RankTotal", typeof(float));
            dt.Columns.Add("Total_Status", typeof(string));

            // مشخصات دانش‌آموز و آزمون
            dt.Columns.Add("StudentCode", typeof(long));
            dt.Columns.Add("Student_FatherName", typeof(string));
            dt.Columns.Add("Student_School_Name", typeof(string));
            dt.Columns.Add("Student_Region_Name", typeof(string));
            dt.Columns.Add("Student_FullName", typeof(string));
            dt.Columns.Add("Student_School_Code", typeof(long));
            dt.Columns.Add("Student_NationalCode", typeof(string));
            dt.Columns.Add("Student_LevelNameString", typeof(string));
            dt.Columns.Add("Student_Section_Name", typeof(string));
            dt.Columns.Add("Pass_school", typeof(string));
            dt.Columns.Add("Pass_Region", typeof(string));
            dt.Columns.Add("Pass_Student", typeof(string));
            dt.Columns.Add("AnswerSheet", typeof(string));
            dt.Columns.Add("KeySheet", typeof(string));
            dt.Columns.Add("ExamStatictics_PresentCount", typeof(int));
            dt.Columns.Add("SchoolStatistics_PresentCount", typeof(int));
            dt.Columns.Add("GenderStatistics_PresentCount", typeof(int));
            dt.Columns.Add("RegionStatistics_PresentCount", typeof(int));
            dt.Columns.Add("ExamName", typeof(string));
            dt.Columns.Add("StringName", typeof(string));
            dt.Columns.Add("tel", typeof(string));
            dt.Columns.Add("GetWeekness", typeof(string));
            dt.Columns.Add("GetStrength", typeof(string));
            dt.Columns.Add("UploadTime", typeof(DateTime));
            dt.Columns.Add("UploadDescription", typeof(string));

            foreach (System.Data.DataColumn col in dt.Columns)
            {
                col.AllowDBNull = true;
            }

            return dt;
        }

        private static void PopulateDataRow(System.Data.DataRow dr, object[] b, DateTime now, string? batchDesc = null)
        {
            int colIdx = 0;
            // دروس ۱ تا ۱۴
            for (int l = 0; l < 14; l++)
            {
                int offset = l * 16;
                dr[colIdx++] = (object?)SafeString(b[offset + 0]) ?? DBNull.Value;
                dr[colIdx++] = SafeFloat(b[offset + 1]);
                dr[colIdx++] = SafeFloat(b[offset + 2]);
                dr[colIdx++] = SafeFloat(b[offset + 3]);
                dr[colIdx++] = SafeFloat(b[offset + 4]);
                dr[colIdx++] = SafeFloat(b[offset + 5]);
                dr[colIdx++] = SafeFloat(b[offset + 6]);
                dr[colIdx++] = SafeFloat(b[offset + 7]);
                dr[colIdx++] = SafeFloat(b[offset + 8]);
                dr[colIdx++] = SafeFloat(b[offset + 9]);
                dr[colIdx++] = SafeFloat(b[offset + 10]);
                dr[colIdx++] = SafeFloat(b[offset + 11]);
                dr[colIdx++] = SafeFloat(b[offset + 12]);
                dr[colIdx++] = SafeFloat(b[offset + 13]);
                dr[colIdx++] = SafeFloat(b[offset + 14]);
                dr[colIdx++] = (object?)SafeString(b[offset + 15]) ?? DBNull.Value;
            }

            // کل (Total) - از ایندکس ۲۲۴
            dr[colIdx++] = (object?)SafeString(b[224]) ?? DBNull.Value;
            dr[colIdx++] = SafeFloat(b[225]);
            dr[colIdx++] = SafeFloat(b[226]);
            dr[colIdx++] = SafeFloat(b[227]);
            dr[colIdx++] = SafeFloat(b[228]);
            dr[colIdx++] = SafeFloat(b[229]);
            dr[colIdx++] = SafeFloat(b[230]);
            dr[colIdx++] = SafeFloat(b[231]);
            dr[colIdx++] = SafeFloat(b[232]);
            dr[colIdx++] = SafeFloat(b[233]);
            dr[colIdx++] = SafeFloat(b[234]);
            dr[colIdx++] = SafeFloat(b[235]);
            dr[colIdx++] = SafeFloat(b[236]);
            dr[colIdx++] = SafeFloat(b[237]);
            dr[colIdx++] = SafeFloat(b[238]);
            dr[colIdx++] = (object?)SafeString(b[239]) ?? DBNull.Value;

            // مشخصات دانش‌آموز و آزمون (ایندکس‌های ۲۴۰ تا ۲۶۲)
            dr[colIdx++] = SafeLong(b[240]);
            dr[colIdx++] = (object?)SafeString(b[241]) ?? DBNull.Value;
            dr[colIdx++] = (object?)SafeString(b[242]) ?? DBNull.Value;
            dr[colIdx++] = (object?)SafeString(b[243]) ?? DBNull.Value;
            dr[colIdx++] = (object?)SafeString(b[244]) ?? DBNull.Value;
            dr[colIdx++] = SafeLong(b[245]);
            dr[colIdx++] = (object?)SafeString(b[246]) ?? DBNull.Value;
            dr[colIdx++] = (object?)SafeString(b[247]) ?? DBNull.Value;
            dr[colIdx++] = (object?)SafeString(b[248]) ?? DBNull.Value;
            dr[colIdx++] = (object?)SafeString(b[249]) ?? DBNull.Value;
            dr[colIdx++] = (object?)SafeString(b[250]) ?? DBNull.Value;
            dr[colIdx++] = (object?)SafeString(b[251]) ?? DBNull.Value;
            dr[colIdx++] = (object?)SafeString(b[252]) ?? DBNull.Value;
            dr[colIdx++] = (object?)SafeString(b[253]) ?? DBNull.Value;
            dr[colIdx++] = SafeInt(b[254]);
            dr[colIdx++] = SafeInt(b[255]);
            dr[colIdx++] = SafeInt(b[256]);
            dr[colIdx++] = SafeInt(b[257]);
            dr[colIdx++] = (object?)SafeString(b[258]) ?? DBNull.Value;
            dr[colIdx++] = (object?)SafeString(b[259]) ?? DBNull.Value;
            dr[colIdx++] = (object?)SafeString(b[260]) ?? DBNull.Value;
            dr[colIdx++] = (object?)SafeString(b[261]) ?? DBNull.Value;
            dr[colIdx++] = (object?)SafeString(b[262]) ?? DBNull.Value;
            dr[colIdx++] = now;
            dr[colIdx++] = (object?)SafeString(batchDesc) ?? DBNull.Value;
        }

        private static float SafeFloat(object? val)
        {
            if (val is null) return 0f;
            if (val is double d) return (float)d;
            if (val is float f) return f;
            if (val is int i) return i;
            if (val is long l) return l;
            if (val is decimal m) return (float)m;

            var s = val.ToString()?.Trim();
            if (string.IsNullOrEmpty(s)) return 0f;

            return float.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float r)
                   || float.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out r)
                ? r : 0f;
        }

        private static long SafeLong(object? val)
        {
            if (val is null) return 0;
            if (val is long l) return l;
            if (val is int i) return i;
            if (val is double d) return (long)d;
            if (val is decimal m) return (long)m;

            var s = val.ToString()?.Trim();
            if (string.IsNullOrEmpty(s)) return 0;

            return long.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out long r) ? r : 0;
        }

        private static int SafeInt(object? val)
        {
            if (val is null) return 0;
            if (val is int i) return i;
            if (val is long l) return (int)l;
            if (val is double d) return (int)d;
            if (val is decimal m) return (int)m;

            var s = val.ToString()?.Trim();
            if (string.IsNullOrEmpty(s)) return 0;

            return int.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out int r) ? r : 0;
        }

        private static string? SafeString(object? val)
        {
            if (val is null) return null;
            var s = val.ToString();
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }


        [HttpPut]
        public async Task<IActionResult> Put(Report_students data)
        {
            _context.Report_students.Update(data);
            await _context.SaveChangesAsync();
            return Ok();

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var dev = await _context.Report_students.FirstOrDefaultAsync(d => d.ID == id);

            if (dev == null)
                return NotFound();

            _context.Report_students.Remove(dev);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
