using Hangfire;
using LectureRecordingManager.Jobs;
using LectureRecordingManager.Jobs.Configuration;
using LectureRecordingManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LectureRecordingManager.Controllers
{
    [ApiController]
    [Authorize]
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
                .OrderBy(x => x.Sorting).ThenBy(x => x.UploadDate)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Recording>> GetRecording(int id)
        {
            var recording = await _context.Recordings
                .Include(x => x.Outputs.Where(y => y.Status != RecordingStatus.DELETED))
                .Where(x => x.Id == id)
                .SingleAsync();

            if (recording == null)
            {
                return NotFound();
            }

            return recording;
        }

        [HttpPost("sorting/{lectureId}")]
        public async Task<ActionResult<IEnumerable<Recording>>> PutSorting(int lectureId, List<int> ids)
        {
            var order = 0;
            foreach (var item in ids)
            {
                var recording = await _context.Recordings.FindAsync(item);
                recording.Sorting = order;
                _context.Entry(recording).State = EntityState.Modified;

                order += 1;
            }

            await _context.SaveChangesAsync();

            return await GetRecordingByLecture(lectureId);
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
            var recording = await _context.Recordings.Include(x => x.Chapters).SingleOrDefaultAsync(x => x.Id == id);
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
            var recording = await _context.Recordings
                .Include(x => x.Lecture)
                .FirstOrDefaultAsync(x => x.Id == recordingId);

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
            BackgroundJob.Enqueue<PreviewRecordingJob>(x => x.Preview(recording.Id));

            // process upload (convert)
            if (process)
            {
                // render 720p
                BackgroundJob.Enqueue<ProcessRecordingJob>(x => x.Execute(new ProcessRecordingJobConfiguration() { RecordingId = recording.Id, OutputType = ProcessRecordingOutputType.Video_720p }));

                // render 1080p
                if (recording.Lecture.RenderFullHd)
                {
                    BackgroundJob.Enqueue<ProcessRecordingJob>(x => x.Execute(new ProcessRecordingJobConfiguration() { RecordingId = recording.Id, OutputType = ProcessRecordingOutputType.Video_1080P }));
                }
            }

            return Ok();
        }

        [HttpGet("process/{id}")]
        public async Task<ActionResult<Recording>> ProcessRecording(int id)
        {
            var recording = await _context.Recordings
                .Include(x => x.Lecture)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (recording == null)
            {
                return NotFound();
            }

            // render 720p
            BackgroundJob.Enqueue<ProcessRecordingJob>(x => x.Execute(new ProcessRecordingJobConfiguration() { RecordingId = recording.Id, OutputType = ProcessRecordingOutputType.Video_720p }));

            // render 1080p
            if (recording.Lecture.RenderFullHd)
            {
                BackgroundJob.Enqueue<ProcessRecordingJob>(x => x.Execute(new ProcessRecordingJobConfiguration() { RecordingId = recording.Id, OutputType = ProcessRecordingOutputType.Video_1080P }));
            }

            return recording;
        }

        [HttpGet("preview/{id}")]
        public async Task<ActionResult<Recording>> PreviewImage(int id)
        {
            var recording = await _context.Recordings
                .Include(x => x.Lecture)
                .Where(x => x.Id == id)
                .Where(x => x.Outputs.Any(x => x.JobType == typeof(PreviewRecordingJob).FullName && x.Status == RecordingStatus.PROCESSED))
                .SingleOrDefaultAsync();

            if (recording == null)
            {
                return NotFound();
            }

            // get file preview
            var previewFileName = Path.Combine(recording.Lecture.ConvertedPath, recording.Id.ToString(), "preview", "thumbnail.jpg");

            if (!System.IO.File.Exists(previewFileName))
            {
                return NotFound();
            }

            var stream = new FileStream(previewFileName, FileMode.Open);
            return File(stream, System.Net.Mime.MediaTypeNames.Image.Jpeg);
        }

        [Route("preview_video/{id}")]
        public async Task<ActionResult> PreviewVideo(int id)
        {
            var recording = await _context.Recordings
                .Include(x => x.Lecture)
                .Where(x => x.Id == id)
                .Where(x => x.Outputs.Any(x => x.JobType == typeof(PreviewRecordingJob).FullName && x.Status == RecordingStatus.PROCESSED))
                .SingleOrDefaultAsync();

            if (recording == null)
            {
                return NotFound();
            }

            // preview video path
            string videoFileName = "";

            if (recording.Type == RecordingType.GREEN_SCREEN_RECORDING || recording.Type == RecordingType.SIMPLE_RECORDING)
            {
                videoFileName = "stage.mp4";
            }
            else if (recording.Type == RecordingType.ZOOM_RECORDING)
            {
                videoFileName = "slides.mp4";
            }

            // get file preview
            var previewFileName = Path.Combine(recording.Lecture.ConvertedPath, recording.Id.ToString(), "preview", videoFileName);

            if (!System.IO.File.Exists(previewFileName))
            {
                return NotFound();
            }

            return PhysicalFile(previewFileName, "application/octet-stream", enableRangeProcessing: true);
        }

        [Route("rendered_video/{id}")]
        public async Task<ActionResult> RenderedVideo(int id)
        {
            var output = await _context.RecordingOutputs
                .Include(x => x.Recording)
                .Include(x => x.Recording.Lecture)
                .Where(x => x.JobType == typeof(ProcessRecordingJob).FullName && (x.Status == RecordingStatus.PROCESSED || x.Status == RecordingStatus.PUBLISHED))
                .SingleOrDefaultAsync(x => x.Id == id);

            if (output == null)
            {
                return NotFound();
            }

            // preview video path
            string videoFileName = "";

            if (output.Recording.Type == RecordingType.GREEN_SCREEN_RECORDING || output.Recording.Type == RecordingType.SIMPLE_RECORDING)
            {
                videoFileName = "stage.mp4";
            }
            else if (output.Recording.Type == RecordingType.ZOOM_RECORDING)
            {
                videoFileName = "slides.mp4";
            }

            // generate target video path
            if (output.Status == RecordingStatus.PROCESSED)
            {
                videoFileName = Path.Combine(output.Recording.Lecture.ConvertedPath, output.RecordingId.ToString(), "output_" + output.Id, videoFileName);
            }
            else if (output.Status == RecordingStatus.PUBLISHED)
            {
                // publish each output separately
                var configuration = JsonConvert.DeserializeObject<ProcessRecordingJobConfiguration>(output.JobConfiguration);
                var publishFolder = Path.Combine(output.Recording.Lecture.PublishPath, "video", output.RecordingId.ToString());

                if (configuration.OutputType == ProcessRecordingOutputType.Default || configuration.OutputType == ProcessRecordingOutputType.Video_720p)
                {
                    videoFileName = Path.Combine(publishFolder, "output_720p", videoFileName);
                }
                else if (configuration.OutputType == ProcessRecordingOutputType.Video_1080P)
                {
                    videoFileName = Path.Combine(publishFolder, "output_1080p", videoFileName);
                }
            }

            if (!System.IO.File.Exists(videoFileName))
            {
                return NotFound();
            }

            return PhysicalFile(videoFileName, "application/octet-stream", enableRangeProcessing: true);
        }

        [HttpGet("{id}/chapters")]
        public async Task<ActionResult<IEnumerable<RecordingChapter>>> GetRecordingChapters(int id)
        {
            return await _context.RecordingChapters
                .Where(x => x.Recording.Id == id)
                .OrderBy(x => x.StartPosition)
                .ToListAsync();
        }

        [HttpGet("preview/chapter/{id}")]
        public async Task<ActionResult> PreviewChapterImage(int id)
        {
            var chapter = await _context.RecordingChapters
                .Include(x => x.Recording)
                .Include(x => x.Recording.Lecture)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (chapter == null)
            {
                return NotFound();
            }

            var stream = new FileStream(Path.Combine(chapter.Recording.Lecture.ConvertedPath, chapter.Recording.Id.ToString(), "preview", chapter.Thumbnail), FileMode.Open);
            return File(stream, System.Net.Mime.MediaTypeNames.Image.Jpeg);
        }

        [HttpGet("publish/{id}")]
        public async Task<ActionResult> PublishRecording(int id)
        {
            // check for recordings that should be published
            var outputs = await _context.RecordingOutputs
                .Where(x => x.Status == RecordingStatus.PROCESSED)
                .Where(x => x.JobType == typeof(ProcessRecordingJob).FullName)
                .Where(x => x.RecordingId == id)
                .ToListAsync();

            foreach (var output in outputs)
            {
                output.Status = RecordingStatus.SCHEDULED_PUBLISH;
                await _context.SaveChangesAsync();

                BackgroundJob.Enqueue<PublishRecordingJob>(x => x.PublishRecordingOutput(output.Id));
            }

            return Ok();
        }

        [HttpGet("previewdo/{id}")]
        public async Task<ActionResult> PreviewdoImage(int id)
        {
            var recording = await _context.Recordings.FindAsync(id);

            if (recording == null)
            {
                return NotFound();
            }

            BackgroundJob.Enqueue<PreviewRecordingJob>(x => x.Preview(id));

            return Ok();
        }

        [HttpDelete("output/{id}")]
        public async Task<ActionResult<Recording>> DeleteOutput(int id)
        {
            var output = await _context.RecordingOutputs
                .Include(x => x.Recording)
                .Include(x => x.Recording.Lecture)
                .SingleOrDefaultAsync(x => x.Id == id);

            if (output == null)
            {
                return NotFound();
            }

            // check if just processed and not published
            if (output.Status != RecordingStatus.PROCESSED || output.JobType != typeof(ProcessRecordingJob).FullName)
            {
                return NotFound();
            }

            // delete output folder
            var outputFolder = Path.Combine(output.Recording.Lecture.ConvertedPath, output.Recording.Id.ToString(), "output_" + output.Id);
            try
            {
                Directory.Delete(outputFolder, true);
            }
            catch (Exception ex)
            {
                output.JobError = ex.Message;
                output.Status = RecordingStatus.ERROR;

                await _context.SaveChangesAsync();
                return Ok();
            }

            // set deleted status
            output.Status = RecordingStatus.DELETED;
            await _context.SaveChangesAsync();

            return await GetRecording(output.Recording.Id);
        }

        private bool RecordingExists(int id)
        {
            return _context.Recordings.Any(e => e.Id == id);
        }
    }
}
