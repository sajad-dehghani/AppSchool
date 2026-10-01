using Azure.Core;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class QController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public QController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("Upload")]
    public async Task<IActionResult> Upload(IFormFile upload)
    {
        if (upload == null || upload.Length == 0)
            return BadRequest();

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = Guid.NewGuid() + Path.GetExtension(upload.FileName);

        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);

        await upload.CopyToAsync(stream);

        var fileUrl =
            $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";

        return Ok(new
        {
            url = fileUrl
        });
    }
}