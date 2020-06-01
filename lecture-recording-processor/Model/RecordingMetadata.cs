using System;

namespace RecordingProcessor.Model
{
    public class RecordingMetadata
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public string FileName { get; set; }

        public string PresenterFileName { get; set; }

        public string StageVideo { get; set; }

        public Slide[] Slides { get; set; }

        public double Duration { get; set; }

        public DateTime Date { get; set; }
    }
}
