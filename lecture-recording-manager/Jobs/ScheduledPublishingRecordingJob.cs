using Hangfire;
using LectureRecordingManager.Hubs;
using LectureRecordingManager.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LectureRecordingManager.Jobs
{
    public class ScheduledPublishingRecordingJob
    {
        private readonly DatabaseContext _context;
        private readonly IHubContext<MessageHub> hub;

        public ScheduledPublishingRecordingJob(DatabaseContext context,
            IHubContext<MessageHub> hub)
        {
            this._context = context;
            this.hub = hub;
        }

        [Queue("meta-queue")]
        public async Task CheckPublishingRecordings()
        {
            // check for recordings that should be published
            var outputs = await _context.RecordingOutputs
                .Where(x => x.Status == RecordingStatus.PROCESSED)
                .Where(x => x.JobType == typeof(ProcessRecordingJob).FullName || x.JobType == "link")
                .Where(x => x.Recording.PublishDate.HasValue && x.Recording.PublishDate < DateTime.Now)
                .Where(x => x.Recording.Lecture.Active)
                .ToListAsync();

            foreach (var output in outputs)
            {
                output.Status = RecordingStatus.SCHEDULED_PUBLISH;
                await _context.SaveChangesAsync();

                BackgroundJob.Enqueue<PublishRecordingJob>(x => x.PublishRecordingOutput(output.Id));
            }

            // send update notification
            if (outputs.Count > 0)
            {
                await UpdateLectureRecordingStatus();
            }
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
