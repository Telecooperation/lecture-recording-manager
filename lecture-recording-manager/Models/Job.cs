using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace lecture_recording_manager.Models
{
    public class Job
    {
        public int Id { get; set; }

        [Column(TypeName ="varchar(255)")]
        public string Type { get; set; }

        [Column(TypeName = "text")]
        public string Configuration { get; set; }

        public JobStatus Status { get; set; }

        public string StatusText { get; set; }

        public DateTime DateStart { get; set; }

        public DateTime DateEnd { get; set; }
    }
}
