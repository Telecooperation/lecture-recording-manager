using Newtonsoft.Json;
using System;

namespace LectureRecordingManager.Models
{
    public class RecordingOutput
    {
        public int Id { get; set; }

        public int RecordingId { get; set; }

        [JsonIgnore]
        public Recording Recording { get; set; }

        public string JobType { get; set; }

        public string JobConfiguration { get; set; }

        public RecordingStatus Status { get; set; }

        public bool Processed { get; set; }

        public string JobError { get; set; }

        public DateTimeOffset? DateStarted { get; set; }

        public DateTimeOffset? DateFinished { get; set; }
    }
}
