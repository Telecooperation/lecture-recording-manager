namespace LectureRecordingManager.Models
{
    public class RecordingChapter
    {
        public int Id { get; set; }

        public Recording Recording { get; set; }

        public double StartPosition { get; set; }

        public string Thumbnail { get; set; }

        public string Text { get; set; }
    }
}
