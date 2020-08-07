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
                .FirstOrDefaultAsync(x => x.Id == recordingId);

            // only publish processed and existing recordings
            if (recording == null || recording.Published)
            {
                return;
            }

            // check if recording was already published
            Directory.CreateDirectory(Path.Combine(recording.Lecture.PublishPath, "video"));
            Directory.CreateDirectory(Path.Combine(recording.Lecture.PublishPath, "assets"));

            var outputFolder = Path.Combine(recording.Lecture.PublishPath, "video", recording.Id.ToString());
            var targetFolderName = recording.Id.ToString();

            if (Directory.Exists(outputFolder) && !Directory.Exists(Path.Combine(recording.FilePath, "output")))
            {
                // ignore publishing
                return;
            }

            // move files to output directory
            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, true);

            Directory.Move(Path.Combine(recording.FilePath, "output"), outputFolder);

            recording.Status = RecordingStatus.PUBLISHED;
            recording.Published = true;
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
