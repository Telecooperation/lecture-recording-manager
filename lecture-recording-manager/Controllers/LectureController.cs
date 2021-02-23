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
    public class LectureController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public LectureController(DatabaseContext context)
        {
            this._context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lecture>>> GetLectures()
        {
            return await _context.Lectures
                .Include(x => x.Semester)
                .ToListAsync();
        }

        [HttpGet("semester/{semesterId}")]
        public async Task<ActionResult<IEnumerable<Lecture>>> GetLectureBySemester(int semesterId)
        {
            return await _context.Lectures
                .Include(x => x.Semester)
                .Where(x => x.Semester.Id == semesterId)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Lecture>> GetLecture(int id)
        {
            var lecture = await _context.Lectures
                .Include(x => x.Semester)
                .Where(x => x.Id == id)
                .SingleAsync();

            if (lecture == null)
            {
                return NotFound();
            }

            return lecture;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLecture(int id, Lecture lecture)
        {
            if (id != lecture.Id)
            {
                return BadRequest();
            }

            var dbLecture = await _context.Lectures.FindAsync(id);
            dbLecture.ShortHand = lecture.ShortHand;
            dbLecture.Title = lecture.Title;
            dbLecture.Description = lecture.Description;
            dbLecture.PublishPath = lecture.PublishPath;
            dbLecture.SourcePath = lecture.SourcePath;
            dbLecture.ConvertedPath = lecture.ConvertedPath;
            dbLecture.RenderFullHd = lecture.RenderFullHd;
            dbLecture.Active = lecture.Active;
            dbLecture.Publish = lecture.Publish;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LectureExists(id))
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
        public async Task<ActionResult<Lecture>> PostLecture(Lecture lecture)
        {
            _context.Lectures.Add(lecture);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLecture), new { id = lecture.Id }, lecture);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Lecture>> DeleteLecture(int id)
        {
            var lecture = await _context.Lectures.FindAsync(id);
            if (lecture == null)
            {
                return NotFound();
            }

            _context.Lectures.Remove(lecture);
            await _context.SaveChangesAsync();

            return lecture;
        }

        [HttpGet("synchronize/{id}")]
        public IActionResult SynchronizeLecture(int id)
        {
            BackgroundJob.Enqueue<SynchronizePublishedRecordingsJob>(x => x.SynchronizePublishedRecordings(id));

            return Ok();
        }

        private bool LectureExists(int id)
        {
            return _context.Lectures.Any(e => e.Id == id);
        }
    }
}
