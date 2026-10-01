using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovinApp.Server.MyContext;
using NovinApp.Shared;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NovinApp.Server.Controllers;



    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly MyAppContext _context;
        public QuestionController(MyAppContext context)
        {
            _context = context;
        }
        //[AllowAnonymous]

        [HttpGet("all")]
        public async Task<IActionResult> Get_all()
        {
            var devs = await _context.Question.ToListAsync();
            if (devs != null)

                return Ok(devs);
            else
                return NotFound();
        }
        //[HttpGet("students")]
        //public async Task<IActionResult> Get_students()
        //{
        //    var devs = await _context.Question.Where(x => x.Rool == "دانش آموز").ToListAsync();
        //    if (devs != null)

        //        return Ok(devs);
        //    else
        //        return NotFound();
        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {

            var dev = await _context.Question.FirstAsync(d => d.Id == id);
            if (dev != null)
                return Ok(dev);
            else
                return NotFound();

        }


        [HttpPost]
        public async Task<IActionResult> Post(Question data)
        {

            _context.Question.Add(data);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> Put(Question data)
        {
            _context.Question.Update(data);
            await _context.SaveChangesAsync();
            return Ok();

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var dev = await _context.Question.FirstOrDefaultAsync(d => d.Id == id);

            if (dev == null)
                return NotFound();

            _context.Question.Remove(dev);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
