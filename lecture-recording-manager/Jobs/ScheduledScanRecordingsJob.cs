using Hangfire;
using LectureRecordingManager.Hubs;
using LectureRecordingManager.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using RecordingProcessor.Metadata;
using RecordingProcessor.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LectureRecordingManager.Jobs
{
    public class ScheduledScanRecordingsJob
    {
        private readonly DatabaseContext _context;
        private readonly IHubContext<MessageHub> hub;

        public ScheduledScanRecordingsJob(DatabaseContext context,
            IHubContext<MessageHub> hub)
        {
            this._context = context;
            this.hub = hub;
        }

        public async Task ScanRecordings()
        {
            var lectures = await _context.Lectures
                .Where(x =>  x.SourcePath != null && x.SourcePath != "")
                .ToListAsync();

            foreach (var lecture in lectures)
            {
                var files = Directory.EnumerateFiles(lecture.SourcePath, "*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    // only process meta files
                    if (!file.EndsWith("_meta.json"))
                    {
                        continue;
                    }

                    // check if all files are available
                    var slides = file.Replace("_meta.json", "_slides.mp4");
                    var th = file.Replace("_meta.json", "_talkinghead.mp4");

                    if (!File.Exists(slides) || !File.Exists(th))
                    {
                        continue;
                    }

                    // obtain file names and check if we have already stored this recording
                    var filePath = Path.GetDirectoryName(file);
                    var targetName = Path.GetFileName(file).Replace("_meta.json", "");

                    var recordingCount = await _context.Recordings
                        .Where(x => x.LectureId == lecture.Id)
                        .Where(x => x.FilePath == filePath)
                        .Where(x => x.CustomTargetName == targetName)
                        .CountAsync();

                    if (recordingCount > 0)
                    {
                        continue;
                    }

                    // check publish folder, if the recording was already processed
                    var published = Directory.Exists(Path.Combine(lecture.PublishPath, "video", targetName));
                    dynamic metadata = JsonConvert.DeserializeObject(File.ReadAllText(file));

                    // create database entry
                    var recording = new Recording()
                    {
                        CustomTargetName = targetName,
                        Title = metadata["description"],
                        Published = published,
                        PublishDate = DateTime.Parse(metadata["lectureDate"].ToString()),
                        Status = published ? RecordingStatus.PUBLISHED : RecordingStatus.UPLOADED,
                        UploadDate = DateTime.ParseExact(targetName, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture),
                        FilePath = Path.GetDirectoryName(file),
                        Lecture = lecture,
                        Type = RecordingType.GREEN_SCREEN_RECORDING
                    };

                    _context.Recordings.Add(recording);
                    await _context.SaveChangesAsync();

                    // process if not yet processed?
                    if (lecture.Active && !published)
                    {
                        // process upload (preview)
                        BackgroundJob.Enqueue<ProcessRecordingJob>(x => x.Preview(recording.Id));

                        // process upload (convert)
                        BackgroundJob.Enqueue<ProcessRecordingJob>(x => x.Execute(recording.Id));
                    }
                }
            }
        }
    }
}
