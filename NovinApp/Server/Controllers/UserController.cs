using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovinApp.Server.Helpers;
using NovinApp.Server.MyContext;
using NovinApp.Shared;

namespace WebApp.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly MyAppContext _context;
        public UserController(MyAppContext context)
        {
            _context = context;
        }

        [HttpGet("all")]
        public async Task<IActionResult> Get_all()
        {
            var devs = await _context.User.ToListAsync();
            return Ok(devs);
        }

        [HttpGet("moshaver")]
        public async Task<IActionResult> Get_moshaver()
        {
            var devs = await _context.User.Where(x => x.Rool == "مشاور").ToListAsync();
            return Ok(devs);
        }

        [HttpGet("students")]
        public async Task<IActionResult> Get_students()
        {
            var devs = await _context.User.Where(x => x.Rool == "دانش آموز").ToListAsync();
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

            // هش کردن امن پسورد در صورت درج کاربر جدید
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
                // در صورت تغییر کلمه عبور، هش جدید تولید می‌شود
                if (!string.IsNullOrWhiteSpace(personel.pass) && personel.pass != existing.pass && !PasswordHelper.IsHashed(personel.pass))
                {
                    personel.pass = PasswordHelper.HashPassword(personel.pass.Trim());
                }
                else if (string.IsNullOrWhiteSpace(personel.pass))
                {
                    personel.pass = existing.pass; // حفظ پسورد قبلی در صورت خالی بودن
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

            _context.User.Remove(dev);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
