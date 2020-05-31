using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lecture_recording_manager.Models
{
    public enum RecordingStatus
    {
        UPLOADED = 0,
        PROCESSING = 1,
        PROCESSED = 2,
        ERROR = -1
    }
}
