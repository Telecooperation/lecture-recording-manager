using Hangfire;
using LectureRecordingManager.Hubs;
using LectureRecordingManager.Jobs.Configuration;
using LectureRecordingManager.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _config;

        public SynchronizePublishedRecordingsJob(DatabaseContext context,
            IHubContext<MessageHub> hub,
            IConfiguration config)
        {
            this._context = context;
            this.hub = hub;
            this._config = config;
        }

        [Queue("meta-queue")]
        public async Task SynchronizePublishedRecordings(int lectureId)
        {
            var lecture = await _context.Lectures
                .Include(x => x.Semester)
                .SingleAsync(x => x.Id == lectureId);

            if (!lecture.Publish)
                return;

            var recordings = await _context.Recordings
                .Include(x => x.Chapters)
                .Include(x => x.Outputs)
                .Where(x => x.LectureId == lectureId)
                .Where(x => x.Outputs.Any(x => x.Status == RecordingStatus.PUBLISHED))
                .OrderBy(x => x.Sorting).ThenBy(x => x.UploadDate)
                .ToListAsync();

            var lectureMetadata = new RecordingProcessor.Model.Lecture()
            {
                Name = lecture.Title,
                Semester = lecture.Semester.Name,
                Recordings = new List<RecordingMetadata>()
            };

            foreach (var recording in recordings)
            {
                var targetFolderName = recording.Lecture.PublishPath + "/video/" + recording.Id.ToString();

                // generate metadata
                if (recording.LinkedRecording != null)
                {
                    // use reference path
                    var linkedRecording = await _context.Recordings
                        .Include(x => x.Lecture)
                        .Include(x => x.Chapters)
                        .Include(x => x.Outputs)
                        .SingleOrDefaultAsync(x => x.Id == recording.LinkedRecording);
                    targetFolderName = linkedRecording.Lecture.PublishPath + "/video/" + linkedRecording.Id.ToString();

                    var metadata = PublishRecording(linkedRecording, recording, targetFolderName);
                    lectureMetadata.Recordings.Add(metadata);
                }
                else
                {
                    var metadata = PublishRecording(recording, recording, targetFolderName);
                    lectureMetadata.Recordings.Add(metadata);
                }
            }

            // update metadata
            var courseJsonMetadataFile = Path.Combine(_config["PublishVideoPath"], lecture.PublishPath, "assets", "lecture.json");
            Directory.CreateDirectory(Path.GetDirectoryName(courseJsonMetadataFile));

            MetadataService.SaveSettings(lectureMetadata, courseJsonMetadataFile);

            // update lecture
            lecture.LastSynchronized = DateTime.Now;
            await _context.SaveChangesAsync();

            await UpdateLectureStatus();
        }

        private RecordingMetadata PublishRecording(Recording recording, Recording metadataRecording, string targetFolderName)
        {
            var legacy = false;

            var metadata = new RecordingMetadata()
            {
                Id = metadataRecording.Title,
                Name = metadataRecording.Title,
                Description = metadataRecording.Description,

                Duration = recording.Duration,
                Date = metadataRecording.PublishDate.Value
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

                // legacy?
                if (configuration.OutputType == ProcessRecordingOutputType.Legacy)
                {
                    metadata.FileName = recording.Lecture.PublishPath +  "/video/" + recording.CustomTargetName + "/slides.mp4";
                    metadata.StageVideo = recording.Lecture.PublishPath + "/video/" + recording.CustomTargetName + "/stage.mp4";
                    metadata.PresenterFileName = recording.Lecture.PublishPath + "/video/" + recording.CustomTargetName + "/talkinghead.mp4";

                    legacy = true;
                }
            }

            metadata.Slides = recording.Chapters.OrderBy(x => x.StartPosition).Select(x => new Slide()
            {
                Thumbnail = (legacy ? "video/" + recording.CustomTargetName + "/" : targetFolderName + "/output_720p/") + x.Thumbnail,
                Ocr = x.Text,
                StartPosition = (float)x.StartPosition
            }).ToArray();

            return metadata;
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
