using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lecture_recording_manager.Models
{
    public enum JobStatus
    {
        ERROR = -1,
        UNSTARTED = 0,
        SCHEDULED = 1,
        RUNNING = 2,
        FINISHED = 3
    }
}
