using System;
using System.Collections.Generic;
using System.Text;

namespace RecordingProcessor.Model
{
    public class Lecture
    {
        public string Name { get; set; }

        public string Semester { get; set; }

        public List<RecordingMetadata> Recordings { get; set; }
    }
}
