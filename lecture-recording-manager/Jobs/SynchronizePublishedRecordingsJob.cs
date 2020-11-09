using LectureRecordingManager.Hubs;
using LectureRecordingManager.Jobs.Configuration;
using LectureRecordingManager.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RecordingProcessor.Metadata;
using RecordingProcessor.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LectureRecordingManager.Jobs
{
    public class SynchronizePublishedRecordingsJob
    {
        private readonly DatabaseContext _context;
        private readonly IHubContext<MessageHub> hub;

        public SynchronizePublishedRecordingsJob(DatabaseContext context,
            IHubContext<MessageHub> hub)
        {
            this._context = context;
            this.hub = hub;
        }

        public async Task SynchronizePublishedRecordings(int lectureId)
        {
            var lecture = await _context.Lectures
                .Include(x => x.Semester)
                .SingleAsync(x => x.Id == lectureId);

            if (!lecture.Active)
                return;

            var recordings = await _context.Recordings
                .Include(x => x.Chapters)
                .Include(x => x.Outputs)
                .Where(x => x.LectureId == lectureId)
                .Where(x => x.Outputs.Any(x => x.Status == RecordingStatus.PUBLISHED))
                .OrderBy(x => x.UploadDate)
                .ToListAsync();

            var lectureMetadata = new RecordingProcessor.Model.Lecture()
            {
                Name = lecture.Title,
                Semester = lecture.Semester.Name,
                Recordings = new List<RecordingMetadata>()
            };

            foreach (var recording in recordings)
            {
                var targetFolderName = "video/" + recording.Id.ToString();

                // generate metadata
                var metadata = new RecordingMetadata()
                {
                    Id = recording.Title,
                    Name = recording.Title,

                    Duration = recording.Duration,
                    Date = recording.PublishDate.Value
                };

                // check outputs
                foreach (var output in recording.Outputs
                    .Where(x => x.Status == RecordingStatus.PUBLISHED)
                    .Where(x => x.JobType == typeof(ProcessRecordingJob).FullName))
                {
                    var configuration = JsonConvert.DeserializeObject<ProcessRecordingJobConfiguration>(output.JobConfiguration);

                    // 720p processed?
                    if (configuration.OutputType == ProcessRecordingOutputType.Default || configuration.OutputType == ProcessRecordingOutputType.Video_720p)
                    {
                        metadata.FileName = targetFolderName + "/output_720p/slides.mp4";
                        metadata.StageVideo = targetFolderName + "/output_720p/stage.mp4";
                        metadata.PresenterFileName = targetFolderName + "/output_720p/talkinghead.mp4";
                    }

                    // 1080p processed?
                    if (configuration.OutputType == ProcessRecordingOutputType.Video_1080P)
                    {
                        metadata.FileNameHd = targetFolderName + "/output_1080p/slides.mp4";
                        metadata.StageVideoHd = targetFolderName + "/output_1080p/stage.mp4";
                        metadata.PresenterFileNameHd = targetFolderName + "/output_1080p/talkinghead.mp4";
                    }
                }

                metadata.Slides = recording.Chapters.OrderBy(x => x.StartPosition).Select(x => new Slide()
                {
                    Thumbnail = targetFolderName + "/output_720p/" + x.Thumbnail,
                    Ocr = x.Text,
                    StartPosition = (float)x.StartPosition
                }).ToArray();

                lectureMetadata.Recordings.Add(metadata);
            }

            // update metadata
            var courseJsonMetadataFile = Path.Combine(lecture.PublishPath, "assets", "lecture.json");
            MetadataService.SaveSettings(lectureMetadata, courseJsonMetadataFile);

            // update lecture
            lecture.LastSynchronized = DateTime.Now;
            await _context.SaveChangesAsync();

            await UpdateLectureStatus();
        }

        private async Task UpdateLectureStatus()
        {
            await hub.Clients.All.SendAsync("StatusChanged", new Message()
            {
                Type = "UPDATE_LECTURE"
            });
        }
    }
}
