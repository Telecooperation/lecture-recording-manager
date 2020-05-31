using System;
using System.Collections.Generic;
using System.Text;

namespace RecordingProcessor.Studio
{
    public struct ConversionConfiguration
    {
        public string SlideVideoPath { get; set; }

        public string TalkingHeadVideoPath { get; set; }

        public string MetadataPath { get; set; }

        public string OutputDirectory { get; set; }

        public string ProjectName { get; set; }

        public RecordingStyle RecordingStyle { get; set; }

        public bool ExportJson { get; set; }
    }
}
