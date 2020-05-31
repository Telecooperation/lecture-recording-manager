﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lecture_recording_manager.Models
{
    public class RecordingChapter
    {
        public int Id { get; set; }

        public Recording Recording { get; set; }

        public long StartPosition { get; set; }

        public string Thumbnail { get; set; }

        public string Text { get; set; }
    }
}
