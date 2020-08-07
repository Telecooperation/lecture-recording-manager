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
                .OrderBy(x => x.UploadDate)
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

            var dbRecording = await _context.Recordings.FindAsync(id);
            dbRecording.Title = recording.Title;
            dbRecording.Description = recording.Description;
            dbRecording.PublishDate = recording.PublishDate;

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
        public async Task<IActionResult> UploadFiles(int recordingId, [FromForm(Name = "process")] bool process, [FromForm(Name = "files")] List<IFormFile> files)
        {
            var recording = await _context.Recordings.FindAsync(recordingId);

            if (recording == null)
            {
                return NotFound();
            }

            // update upload date and default file path
            recording.UploadDate = DateTime.Now;
            recording.FilePath = Path.Combine(_config["UploadVideoPath"], recording.Id.ToString());
            await _context.SaveChangesAsync();

            // process files
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    Directory.CreateDirectory(recording.FilePath);

                    using (var stream = System.IO.File.Create(Path.Combine(recording.FilePath, file.FileName)))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }

            // process upload (preview)
            BackgroundJob.Enqueue<ProcessRecordingJob>(x => x.Preview(recording.Id));

            // process upload (convert)
            if (process)
            {
                BackgroundJob.Enqueue<ProcessRecordingJob>(x => x.Execute(recording.Id));
            }

            return Ok();
        }

        [HttpGet("process/{id}")]
        public async Task<ActionResult<Recording>> ProcessRecording(int id)
        {
            var recording = await _context.Recordings.FindAsync(id);

            if (recording == null)
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

            var stream = new FileStream(Path.Combine(recording.FilePath, "preview", "thumbnail.jpg"), FileMode.Open);
            return File(stream, System.Net.Mime.MediaTypeNames.Image.Jpeg);
        }

        [HttpGet("{id}/chapters")]
        public async Task<ActionResult<IEnumerable<RecordingChapter>>> GetRecordingChapters(int id)
        {
            return await _context.RecordingChapters
                .Where(x => x.Recording.Id == id)
                .ToListAsync();
        }

        [HttpGet("preview/chapter/{id}")]
        public async Task<ActionResult> PreviewChapterImage(int id)
        {
            var chapter = await _context.RecordingChapters
                .Include(x => x.Recording)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (chapter == null)
            {
                return NotFound();
            }

            var stream = new FileStream(Path.Combine(chapter.Recording.FilePath, chapter.Thumbnail), FileMode.Open);
            return File(stream, System.Net.Mime.MediaTypeNames.Image.Jpeg);
        }

        [HttpGet("publish/{id}")]
        public async Task<ActionResult<Recording>> PublishRecording(int id)
        {
            var recording = await _context.Recordings
                .FindAsync(id);

            if (recording == null || recording.Status != RecordingStatus.PROCESSED)
            {
                return NotFound();
            }

            // schedule publish
            recording.Status = RecordingStatus.SCHEDULED_PUBLISH;
            await _context.SaveChangesAsync();

            BackgroundJob.Enqueue<PublishRecordingJob>(x => x.PublishRecording(recording.Id));

            return recording;
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
