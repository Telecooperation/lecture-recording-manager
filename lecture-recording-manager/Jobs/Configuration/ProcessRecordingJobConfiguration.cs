using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LectureRecordingManager.Jobs.Configuration
{
    public class ProcessRecordingJobConfiguration
    {
        public int RecordingId { get; set; }

        public ProcessRecordingOutputType OutputType { get; set; }
    }
}
