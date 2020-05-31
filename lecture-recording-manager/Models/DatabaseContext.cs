using Microsoft.EntityFrameworkCore;

namespace lecture_recording_manager.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<Lecture> Lectures { get; set; }

        public DbSet<Semester> Semesters { get; set; }

        public DbSet<Recording> Recordings { get; set; }

        public DbSet<RecordingChapter> RecordingChapters { get; set; }
    }
}
