using Azure.Core;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    public UploadController(IWebHostEnvironment env) => _env = env;

    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile upload)
    {
        if (upload == null || upload.Length == 0)
            return BadRequest("فایلی ارسال نشده است.");

        // مسیر ذخیره‌سازی فایل‌ها (پوشه wwwroot/uploads)
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        // تولید نام یکتا برای فایل
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(upload.FileName);
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await upload.CopyToAsync(stream);
        }

        // ساخت آدرس قابل دسترس برای فایل آپلود شده
        var imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
        return Ok(new { url = imageUrl });
    }
}