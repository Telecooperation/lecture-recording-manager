using Hangfire;
using LectureRecordingManager.Hubs;
using LectureRecordingManager.Jobs.Configuration;
using LectureRecordingManager.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.IO;
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

        [Queue("publish-queue")]
        public async Task PublishRecordingOutput(int outputId)
        {
            var output = await _context.RecordingOutputs
                .Include(x => x.Recording)
                .Include(x => x.Recording.Lecture)
                .FirstOrDefaultAsync(x => x.Id == outputId);

            // only publish processed and existing recordings
            if (output == null ||
                output.JobType != typeof(ProcessRecordingJob).FullName ||
                (output.Status != RecordingStatus.PROCESSED && output.Status != RecordingStatus.SCHEDULED_PUBLISH))
            {
                return;
            }

            // check if recording was already published
            Directory.CreateDirectory(Path.Combine(output.Recording.Lecture.PublishPath, "video"));
            Directory.CreateDirectory(Path.Combine(output.Recording.Lecture.PublishPath, "assets"));

            var publishFolder = Path.Combine(output.Recording.Lecture.PublishPath, "video", output.RecordingId.ToString());
            Directory.CreateDirectory(publishFolder);

            // publish each output separately
            var outputFolder = Path.Combine(output.Recording.Lecture.ConvertedPath, output.RecordingId.ToString(), "output_" + output.Id);
            var configuration = JsonConvert.DeserializeObject<ProcessRecordingJobConfiguration>(output.JobConfiguration);

            if (configuration.OutputType == ProcessRecordingOutputType.Default || configuration.OutputType == ProcessRecordingOutputType.Video_720p)
            {
                var publishFolderOut = Path.Combine(publishFolder, "output_720p");

                if (Directory.Exists(publishFolderOut))
                    Directory.Delete(publishFolderOut, true);

                Directory.Move(outputFolder, publishFolderOut);

                output.Status = RecordingStatus.PUBLISHED;
                output.Recording.Published = true;
            }
            else if (configuration.OutputType == ProcessRecordingOutputType.Video_1080P)
            {
                var publishFolderOut = Path.Combine(publishFolder, "output_1080p");

                if (Directory.Exists(publishFolderOut))
                    Directory.Delete(publishFolderOut, true);

                Directory.Move(outputFolder, publishFolderOut);

                output.Status = RecordingStatus.PUBLISHED;
                output.Recording.Published = true;
            }

            if (!output.Recording.PublishDate.HasValue)
            {
                output.Recording.PublishDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            await UpdateLectureRecordingStatus();
            BackgroundJob.Enqueue<SynchronizePublishedRecordingsJob>(x => x.SynchronizePublishedRecordings(output.Recording.LectureId));
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
