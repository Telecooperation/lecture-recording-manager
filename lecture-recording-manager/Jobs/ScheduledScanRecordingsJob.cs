using Hangfire;
using LectureRecordingManager.Hubs;
using LectureRecordingManager.Jobs.Configuration;
using LectureRecordingManager.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LectureRecordingManager.Jobs
{
    public class ScheduledScanRecordingsJob
    {
        private readonly ILogger<ScheduledScanRecordingsJob> _logger;
        private readonly DatabaseContext _context;
        private readonly IHubContext<MessageHub> hub;

        public ScheduledScanRecordingsJob(DatabaseContext context,
            IHubContext<MessageHub> hub,
            ILogger<ScheduledScanRecordingsJob> _logger)
        {
            this._context = context;
            this.hub = hub;
            this._logger = _logger;
        }

        [Queue("meta-queue")]
        public async Task ScanRecordings()
        {
            var lectures = await _context.Lectures
                .Where(x => x.SourcePath != null && x.SourcePath != "")
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

                    // retrieve current sorting number
                    int recordingSortingMax = 0;

                    try
                    {
                        recordingSortingMax = await _context.Recordings
                            .Where(x => x.LectureId == lecture.Id)
                            .MaxAsync(x => x.Sorting);
                    }
                    catch
                    {
                        // can happen when list is empty
                    }

                    // check publish folder, if the recording was already processed
                    var published = Directory.Exists(Path.Combine(lecture.PublishPath, "video", targetName));
                    dynamic metadata = JsonConvert.DeserializeObject(File.ReadAllText(file));

                    try
                    {
                        // create database entry
                        var recording = new Recording()
                        {
                            CustomTargetName = targetName,
                            Title = metadata["description"],
                            Published = published,
                            PublishDate = DateTime.Parse(metadata["lectureDate"].ToString()),
                            UploadDate = DateTime.ParseExact(targetName, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture),
                            FilePath = Path.GetDirectoryName(file),
                            Lecture = lecture,
                            Type = RecordingType.GREEN_SCREEN_RECORDING,
                            Sorting = recordingSortingMax + 1
                        };

                        if (string.IsNullOrEmpty(recording.Title))
                        {
                            continue;
                        }

                        _context.Recordings.Add(recording);
                        await _context.SaveChangesAsync();

                        // process if not yet processed?
                        if (lecture.Active && !published)
                        {
                            // process upload (preview)
                            BackgroundJob.Enqueue<PreviewRecordingJob>(x => x.Preview(recording.Id));

                            // render 720p
                            BackgroundJob.Enqueue<ProcessRecordingJob>(x => x.Execute(new ProcessRecordingJobConfiguration() { RecordingId = recording.Id, OutputType = ProcessRecordingOutputType.Video_720p }));

                            // render 1080p
                            if (lecture.RenderFullHd)
                            {
                                BackgroundJob.Enqueue<ProcessRecordingJob>(x => x.Execute(new ProcessRecordingJobConfiguration() { RecordingId = recording.Id, OutputType = ProcessRecordingOutputType.Video_1080P }));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Could not process {file}", file);
                    }
                }
            }
        }
    }
}
