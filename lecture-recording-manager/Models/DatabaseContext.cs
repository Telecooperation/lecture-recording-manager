using Microsoft.EntityFrameworkCore;

namespace LectureRecordingManager.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base()
        {
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<Lecture> Lectures { get; set; }

        public DbSet<Semester> Semesters { get; set; }

        public DbSet<Recording> Recordings { get; set; }

        public DbSet<RecordingChapter> RecordingChapters { get; set; }
    }
}
