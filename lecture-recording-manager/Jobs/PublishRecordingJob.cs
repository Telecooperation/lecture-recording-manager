using Hangfire;
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
                .FirstOrDefaultAsync(x => x.Id == recordingId && (x.Status == RecordingStatus.SCHEDULED_PUBLISH || x.FullHdStatus == RecordingStatus.SCHEDULED_PUBLISH));

            // only publish processed and existing recordings
            if (recording == null)
            {
                return;
            }

            // check if recording was already published
            Directory.CreateDirectory(Path.Combine(recording.Lecture.PublishPath, "video"));
            Directory.CreateDirectory(Path.Combine(recording.Lecture.PublishPath, "assets"));

            var publishFolder = Path.Combine(recording.Lecture.PublishPath, "video", recording.Id.ToString());
            Directory.CreateDirectory(publishFolder);

            var output720pFolder = Path.Combine(recording.Lecture.ConvertedPath, recording.Id.ToString(), "output_720p");
            var output1080pFolder = Path.Combine(recording.Lecture.ConvertedPath, recording.Id.ToString(), "output_1080p");

            // publish 720p?
            if (recording.Status == RecordingStatus.SCHEDULED_PUBLISH && Directory.Exists(output720pFolder))
            {
                var publish720pFolder = Path.Combine(publishFolder, "output_720p");

                if (Directory.Exists(publish720pFolder))
                    Directory.Delete(publish720pFolder, true);

                Directory.Move(output720pFolder, Path.Combine(publishFolder, "output_720p"));

                recording.Status = RecordingStatus.PUBLISHED;
                recording.Published = true;
            }

            // publish 1080p
            if (recording.FullHdStatus == RecordingStatus.SCHEDULED_PUBLISH && Directory.Exists(output1080pFolder))
            {
                var publish1080pFolder = Path.Combine(publishFolder, "output_1080p");

                if (Directory.Exists(publish1080pFolder))
                    Directory.Delete(publish1080pFolder, true);

                Directory.Move(output1080pFolder, Path.Combine(publishFolder, "output_1080p"));

                recording.FullHdStatus = RecordingStatus.PUBLISHED;
                recording.Published = true;
            }

            if (!recording.PublishDate.HasValue)
            {
                recording.PublishDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            await UpdateLectureRecordingStatus();
            BackgroundJob.Enqueue<SynchronizePublishedRecordingsJob>(x => x.SynchronizePublishedRecordings(recording.LectureId));
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
