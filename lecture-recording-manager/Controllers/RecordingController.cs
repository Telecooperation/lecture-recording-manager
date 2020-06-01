using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using LectureRecordingManager.Jobs;
using LectureRecordingManager.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LectureRecordingManager.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class RecordingController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IConfiguration _config;

        public RecordingController(DatabaseContext context, IConfiguration config)
        {
            this._context = context;
            this._config = config;
        }

        [HttpGet("lecture/{lectureId}")]
        public async Task<ActionResult<IEnumerable<Recording>>> GetRecordingByLecture(int lectureId)
        {
            return await _context.Recordings
                .Where(x => x.LectureId == lectureId)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Recording>> GetRecording(int id)
        {
            var recording = await _context.Recordings
                .Where(x => x.Id == id)
                .SingleAsync();

            if (recording == null)
            {
                return NotFound();
            }

            return recording;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecording(int id, Recording recording)
        {
            if (id != recording.Id)
            {
                return BadRequest();
            }

            _context.Entry(recording).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecordingExists(id))
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
        public async Task<ActionResult<Recording>> PostRecording(Recording recording)
        {
            _context.Recordings.Add(recording);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRecording), new { id = recording.Id }, recording);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Recording>> DeleteRecording(int id)
        {
            var recording = await _context.Recordings.FindAsync(id);
            if (recording == null)
            {
                return NotFound();
            }

            _context.Recordings.Remove(recording);
            await _context.SaveChangesAsync();

            return recording;
        }

        [HttpPost("upload/{recordingId}"), DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFiles(int recordingId, [FromForm(Name ="files")] List<IFormFile> files)
        {
            var recording = await _context.Recordings.FindAsync(recordingId);

            if (recording == null)
            {
                return NotFound();
            }

            // update upload date
            recording.UploadDate = DateTime.Now;
            await _context.SaveChangesAsync();

            // process files
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var filePath = Path.Combine(_config["UploadVideoPath"], recording.Id.ToString());
                    Directory.CreateDirectory(filePath);

                    using (var stream = System.IO.File.Create(Path.Combine(filePath, file.FileName)))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }

            // process upload
            BackgroundJob.Enqueue<ProcessRecordingJob>(x => x.Preview(recording.Id));

            return Ok();
        }

        [HttpGet("process/{id}")]
        public async Task<ActionResult<Recording>> ProcessRecording(int id)
        {
            var recording = await _context.Recordings.FindAsync(id);

            if(recording == null)
            {
                return NotFound();
            }

            // schedule recording
            recording.Status = RecordingStatus.SCHEDULED;
            await _context.SaveChangesAsync();

            // enqueue processing
            BackgroundJob.Enqueue<ProcessRecordingJob>(x => x.Execute(id));

            return recording;
        }

        [HttpGet("preview/{id}")]
        public async Task<ActionResult<Recording>> PreviewImage(int id)
        {
            var recording = await _context.Recordings.FindAsync(id);

            if (recording == null && recording.Preview)
            {
                return NotFound();
            }

            var stream = new FileStream(Path.Combine(_config["UploadVideoPath"], id.ToString(), "preview", "thumbnail.jpg"), FileMode.Open);
            return File(stream, System.Net.Mime.MediaTypeNames.Image.Jpeg);
        }

        [HttpGet("previewdo/{id}")]
        public async Task<ActionResult<Recording>> PreviewdoImage(int id)
        {
            var recording = await _context.Recordings.FindAsync(id);

            if (recording == null)
            {
                return NotFound();
            }

            BackgroundJob.Enqueue<ProcessRecordingJob>(x => x.Preview(id));

            return Ok();
        }

        private bool RecordingExists(int id)
        {
            return _context.Recordings.Any(e => e.Id == id);
        }
    }
}
