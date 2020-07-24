using Hangfire;
using LectureRecordingManager.Hubs;
using LectureRecordingManager.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        public async Task CheckPublishingRecordings()
        {
            // check for recordings that should be published
            var recordings = await _context.Recordings
                .Where(x => x.Status == RecordingStatus.PROCESSED)
                .Where(x => x.Lecture.Active)
                .Where(x => x.PublishDate.HasValue && x.PublishDate < DateTime.Now)
                .ToListAsync();

            foreach (var recording in recordings)
            {
                var rec = await _context.Recordings.FindAsync(recording.Id);
                // schedule publish
                rec.Status = RecordingStatus.SCHEDULED_PUBLISH;
                await _context.SaveChangesAsync();

                BackgroundJob.Enqueue<PublishRecordingJob>(x => x.PublishRecording(recording.Id));
            }

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
