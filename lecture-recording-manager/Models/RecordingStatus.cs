using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LectureRecordingManager.Models
{
    public enum RecordingStatus
    {
        UPLOADED = 0,
        PROCESSING = 1,
        PROCESSED = 2,
        ERROR = -1
    }
}
