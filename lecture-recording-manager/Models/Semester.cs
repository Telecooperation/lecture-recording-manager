using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace lecture_recording_manager.Models
{
    public class Semester
    {
        public int Id { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string Name { get; set; }

        public DateTime DateStart { get; set; }

        public DateTime DateEnd { get; set; }

        public bool Published { get; set; }

        public bool Active { get; set; }
    }
}
