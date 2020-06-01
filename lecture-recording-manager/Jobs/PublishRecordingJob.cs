using LectureRecordingManager.Hubs;
using LectureRecordingManager.Models;
using LectureRecordingManager.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RecordingProcessor.Metadata;
using RecordingProcessor.Model;
using SixLabors.ImageSharp.ColorSpaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LectureRecordingManager.Jobs
{
    public class PublishRecordingJob
    {
        private readonly DatabaseContext _context;
        private readonly IHubContext<MessageHub> hub;

        public PublishRecordingJob(DatabaseContext context,
            IHubContext<MessageHub> hub)
        {
            this._context = context;
            this.hub = hub;
        }

        public async Task PublishRecording(int recordingId)
        {
            var recording = await _context.Recordings
                .Include(x => x.Lecture)
                .Include(x => x.Lecture.Semester)
                .Include(x => x.Chapters)
                .FirstOrDefaultAsync(x => x.Id == recordingId);

            // only publish processed and existing recordings
            if (recording == null || recording.Published)
            {
                return;
            }

            // check if recording was already published
            Directory.CreateDirectory(Path.Combine(recording.Lecture.PublishPath, "video"));
            Directory.CreateDirectory(Path.Combine(recording.Lecture.PublishPath, "assets"));

            var outputFolder = Path.Combine(recording.Lecture.PublishPath, "video", recording.UploadDate.ToString("yyyy-MM-dd-HH-mm-ss"));
            var targetFolderName = recording.UploadDate.ToString("yyyy-MM-dd-HH-mm-ss");

            if (Directory.Exists(outputFolder))
            {
                // ignore publishing
                return;
            }

            // move files to output directory
            Directory.Move(Path.Combine(recording.FilePath, "output"), outputFolder);

            // generate metadata
            if (recording.Type == RecordingType.GREEN_SCREEN_RECORDING)
            {
                var metadata = new RecordingMetadata()
                {
                    Id = recording.Title,
                    Name = recording.Title,
                    FileName = targetFolderName + "/slides.mp4",
                    StageVideo = targetFolderName + "/stage.mp4",
                    PresenterFileName = targetFolderName + "/talkinghead.mp4",
                    Duration = recording.Duration,
                    Date = recording.PublishDate
                };

                metadata.Slides = recording.Chapters.Select(x => new Slide()
                {
                    Thumbnail = targetFolderName + "/" + x.Thumbnail,
                    Ocr = x.Text,
                    StartPosition = (float)x.StartPosition
                }).ToArray();

                var courseJsonMetadataFile = Path.Combine(recording.Lecture.PublishPath, "assets", "lecture.json");
                var lecture = MetadataService.LoadSettings(recording.Lecture.Title, recording.Lecture.Semester.Name, courseJsonMetadataFile);
                lecture.Recordings.Add(metadata);

                MetadataService.SaveSettings(lecture, courseJsonMetadataFile);
            }

            recording.Status = RecordingStatus.PUBLISHED;
            recording.Published = true;
            await _context.SaveChangesAsync();
            await UpdateLectureRecordingStatus();
        }

        private async Task UpdateLectureRecordingStatus()
        {
            await hub.Clients.All.SendAsync("StatusChanged", new Message()
            {
                Type = "UPDATE_LECTURE_RECORDING_STATUS"
            });
        }
    }
}
