using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LectureRecordingManager.Models
{
    public class Recording
    {
        public int Id { get; set; }

        public int LectureId { get; set; }

        public Lecture Lecture { get; set; }

        public RecordingType Type { get; set; }

        [Required]
        [Column(TypeName = "varchar(255)")]
        public string Title { get; set; }

        public string Description { get; set; }

        public double Duration { get; set; }

        public bool Published { get; set; }

        public RecordingStatus Status { get; set; }

        public string StatusText { get; set; }

        public DateTime UploadDate { get; set; }

        public DateTime PublishDate { get; set; }

        public int Sorting { get; set; }

        public bool Preview { get; set; }

        public string FilePath { get; set; }

        public List<RecordingChapter> Chapters { get; set; }
    }
}
