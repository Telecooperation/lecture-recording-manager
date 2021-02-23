using Hangfire;
using LectureRecordingManager.Jobs;
using LectureRecordingManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LectureRecordingManager.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class SemesterController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public SemesterController(DatabaseContext context)
        {
            this._context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Semester>>> GetSemesters()
        {
            return await _context.Semesters.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Semester>> GetSemester(int id)
        {
            var semester = await _context.Semesters.FindAsync(id);

            if (semester == null)
            {
                return NotFound();
            }

            return semester;
        }

        [HttpGet("current")]
        public async Task<ActionResult<Semester>> GetCurrentSemester()
        {
            var semester = await _context.Semesters
                .Where(x => x.Active)
                .FirstOrDefaultAsync();

            if (semester == null)
            {
                return NotFound();
            }

            return semester;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSemester(int id, Semester semester)
        {
            if (id != semester.Id)
            {
                return BadRequest();
            }

            _context.Entry(semester).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SemesterExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Semester>> PostSemester(Semester semester)
        {
            _context.Semesters.Add(semester);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSemester), new { id = semester.Id }, semester);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Semester>> DeleteSemester(int id)
        {
            var semester = await _context.Semesters.FindAsync(id);
            if (semester == null)
            {
                return NotFound();
            }

            _context.Semesters.Remove(semester);
            await _context.SaveChangesAsync();

            return semester;
        }

        [HttpGet("synchronize")]
        public ActionResult SynchronizeCourses()
        {
            BackgroundJob.Enqueue<SynchronizePublishedLecturesJob>(x => x.SynchronizeLectures());

            return Ok();
        }

        private bool SemesterExists(int id)
        {
            return _context.Semesters.Any(e => e.Id == id);
        }
    }
}
