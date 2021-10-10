using Hangfire;
using LectureRecordingManager.Jobs;
using LectureRecordingManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

        //[HttpGet("import")]
        //public async Task<IActionResult> ImportLecture()
        //{
        //    var lecture = await _context.Lectures.FindAsync(1);

        //    var metaFiles = System.IO.Directory.GetFiles("\\\\teaching.tk.informatik.tu-darmstadt.de\\recordings\\Public\\videos\\TK3\\SoSe20-Ex\\video", "*.json", System.IO.SearchOption.AllDirectories);

        //    foreach (var metaFile in metaFiles)
        //    {
        //        var meta = JsonConvert.DeserializeObject<RecordingProcessor.Model.RecordingMetadata>(System.IO.File.ReadAllText(metaFile));
        //        var folder = System.IO.Path.GetDirectoryName(metaFile);

        //        var rec = new Recording()
        //        {
        //            Lecture = lecture,
        //            Title = meta.Name,
        //            Description = meta.Description,
        //            PublishDate = meta.Date,
        //            UploadDate = meta.Date,
        //            Duration = meta.Duration,
        //            Published = true,
        //            Type = RecordingType.GREEN_SCREEN_RECORDING,
        //            CustomTargetName = folder.Substring(folder.LastIndexOf('\\') + 1),
        //            Chapters = meta.Slides?.Select(x => new RecordingChapter()
        //            {
        //                Text = x.Ocr,
        //                Thumbnail = x.Thumbnail.Replace(folder, ""),
        //                StartPosition = x.StartPosition
        //            }).ToList(),
        //            Outputs = new List<RecordingOutput>()
        //                        {
        //                            new RecordingOutput()
        //                            {
        //                                JobType = "LectureRecordingManager.Jobs.ProcessRecordingJob",
        //                                DateStarted = meta.Date,
        //                                DateFinished = meta.Date,
        //                                Processed = true,
        //                                Status = RecordingStatus.PROCESSED,
        //                                JobConfiguration = "{ \"OutputType\": 3 }"
        //                            }
        //                        },
        //            Sorting = 0
        //        };

        //        _context.Recordings.Add(rec);
        //    }

        //    await _context.SaveChangesAsync();
        //    return Ok();
        //}

        //[HttpGet("import")]
        //public async Task<IActionResult> ImportLecture()
        //{
        //    var lecture = await _context.Lectures.FindAsync(1);

        //    var file = JsonConvert.DeserializeObject<RecordingProcessor.Model.Lecture>(System.IO.File.ReadAllText("C:\\Users\\AlexanderSeeliger\\Git\\lecture-recording-manager\\lecture.json"));

        //    foreach (var recording in file.Recordings)
        //    {
        //        var folder = (!string.IsNullOrEmpty(recording.FileName)) ? recording.FileName.Replace("/slides.mp4", "") : recording.StageVideo.Replace("/stage_720p.mp4", "");

        //        var rec = new Recording()
        //        {
        //            Lecture = lecture,
        //            Title = recording.Name,
        //            Description = recording.Description,
        //            PublishDate = recording.Date,
        //            UploadDate = recording.Date,
        //            Duration = recording.Duration,
        //            Published = true,
        //            Type = RecordingType.GREEN_SCREEN_RECORDING,
        //            CustomTargetName = folder.Substring(folder.LastIndexOf('/') + 1),
        //            Chapters = recording?.Slides?.Select(x => new RecordingChapter()
        //            {
        //                Text = x.Ocr,
        //                Thumbnail = x.Thumbnail.Replace(folder, ""),
        //                StartPosition = x.StartPosition
        //            }).ToList(),
        //            Outputs = new List<RecordingOutput>()
        //            {
        //                new RecordingOutput()
        //                {
        //                    JobType = "LectureRecordingManager.Jobs.ProcessRecordingJob",
        //                    DateStarted = recording.Date,
        //                    DateFinished = recording.Date,
        //                    Processed = true,
        //                    Status = RecordingStatus.PROCESSED,
        //                    JobConfiguration = "{ \"OutputType\": 3 }"
        //                }
        //            },
        //            Sorting = 0
        //        };

        //        _context.Recordings.Add(rec);
        //    }

        //    await _context.SaveChangesAsync();

        //    return Ok();
        //}

        private bool LectureExists(int id)
        {
            return _context.Lectures.Any(e => e.Id == id);
        }
    }
}
