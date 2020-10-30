using LectureRecordingManager.Authentication;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LectureRecordingManager.Models
{
    public class DatabaseContext : IdentityDbContext<ApplicationUser>
    {
        public DatabaseContext() : base()
        {

        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Lecture> Lectures { get; set; }

        public DbSet<Semester> Semesters { get; set; }

        public DbSet<Recording> Recordings { get; set; }

        public DbSet<RecordingChapter> RecordingChapters { get; set; }
    }
}
