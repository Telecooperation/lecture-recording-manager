using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lecture_recording_manager.Models.Dto
{
    public class LectureCreateOrEditDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int SemesterId { get; set; }

        public bool Publish { get; set; }

        public bool Active { get; set; }
    }
}
