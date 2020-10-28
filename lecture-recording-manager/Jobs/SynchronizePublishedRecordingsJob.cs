using LectureRecordingManager.Hubs;
using LectureRecordingManager.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
                .Where(x => x.LectureId == lectureId)
                .Where(x => x.Published)
                .ToListAsync();

            var lectureMetadata = new RecordingProcessor.Model.Lecture()
            {
                Name = lecture.Title,
                Semester = lecture.Semester.Name,
                Recordings = new List<RecordingMetadata>()
            };

            foreach (var recording in recordings)
            {
                var targetFolderName = !string.IsNullOrEmpty(recording.CustomTargetName) ? recording.CustomTargetName : recording.Id.ToString();

                // generate metadata
                if (recording.Type == RecordingType.GREEN_SCREEN_RECORDING || recording.Type == RecordingType.SIMPLE_RECORDING)
                {
                    var metadata = new RecordingMetadata()
                    {
                        Id = recording.Title,
                        Name = recording.Title,
                       
                        Duration = recording.Duration,
                        Date = recording.PublishDate.Value
                    };

                    // 720p processed?
                    if(recording.Status == RecordingStatus.PUBLISHED)
                    {
                        metadata.FileName = targetFolderName + "/output_720p/slides.mp4";
                        metadata.StageVideo = targetFolderName + "/output_720p/stage.mp4";
                        metadata.PresenterFileName = targetFolderName + "/output_720p/talkinghead.mp4";
                    }

                    // 1080p processed?
                    if (recording.FullHdStatus == RecordingStatus.PUBLISHED)
                    {
                        metadata.FileNameHd = targetFolderName + "/output_1080p/slides.mp4";
                        metadata.StageVideoHd = targetFolderName + "/output_1080p/stage.mp4";
                        metadata.PresenterFileNameHd = targetFolderName + "/output_1080p/talkinghead.mp4";
                    }

                    metadata.Slides = recording.Chapters.Select(x => new Slide()
                    {
                        Thumbnail = targetFolderName + "/output_720p/" + x.Thumbnail,
                        Ocr = x.Text,
                        StartPosition = (float)x.StartPosition
                    }).ToArray();

                    lectureMetadata.Recordings.Add(metadata);
                }
                else if (recording.Type == RecordingType.ZOOM_RECORDING)
                {
                    var metadata = new RecordingMetadata()
                    {
                        Id = recording.Title,
                        Name = recording.Title,
                        FileName = targetFolderName + "/output720p/slides.mp4",
                        Duration = recording.Duration,
                        Date = recording.PublishDate.Value
                    };

                    metadata.Slides = recording.Chapters.Select(x => new Slide()
                    {
                        Thumbnail = targetFolderName + "/" + x.Thumbnail,
                        Ocr = x.Text,
                        StartPosition = (float)x.StartPosition
                    }).ToArray();

                    lectureMetadata.Recordings.Add(metadata);
                }
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
