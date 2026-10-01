using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovinApp.Server.MyContext;
using NovinApp.Shared;
using static System.Collections.Specialized.BitVector32;

namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly MyAppContext _context;
        public UserController(MyAppContext context)
        {
            _context = context;
        }
        //[AllowAnonymous]

        [HttpGet("all")]
        public async Task<IActionResult> Get_all()
        {
            var devs = await _context.User.Where(x=>x.Id==1).ToListAsync();
            if (devs != null)

                return Ok(devs);
            else
                return NotFound();
        }
        [HttpGet("moshaver")]
        public async Task<IActionResult> Get_moshaver()
        {
            var devs = await _context.User.Where(x=>x.Rool=="مشاور").ToListAsync();
            if (devs != null)

                return Ok(devs);
            else
                return NotFound();
        }
        [HttpGet("students")]
        public async Task<IActionResult> Get_students()
        {
            var devs = await _context.User.Where(x => x.Rool == "دانش آموز").ToListAsync();
            if (devs != null)

                return Ok(devs);
            else
                return NotFound();
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

            var dev = await _context.User.Where(x => x.code_meli == code).ToListAsync();
            if (dev != null && dev.Count > 0)
                return Ok(dev);
            else
                return NotFound();

        }
        [HttpPost]
        public async Task<IActionResult> Post(User personel)
        {

            _context.User.Add(personel);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> Put(User personel)
        {
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
