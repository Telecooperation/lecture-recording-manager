using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LectureRecordingManager.Models
{
    public class Semester
    {
        public int Id { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string Name { get; set; }

        public DateTimeOffset DateStart { get; set; }

        public DateTimeOffset DateEnd { get; set; }

        public bool Published { get; set; }

        public bool Active { get; set; }
    }
}
