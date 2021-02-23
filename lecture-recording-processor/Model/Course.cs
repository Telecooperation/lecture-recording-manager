namespace RecordingProcessor.Model
{
    public class Course
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Folder { get; set; }

        public string Semester { get; set; }

        public bool Current { get; set; }

        public bool WeekView { get; set; }

        public bool PublishMode { get; set; }
    }
}
