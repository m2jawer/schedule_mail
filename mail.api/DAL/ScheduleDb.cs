using mail.api.Model;
using Microsoft.EntityFrameworkCore;

namespace mail.api.DAL
{
    public class ScheduleDb : DbContext
    {
        public DbSet<SysSettings> SysSettings { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<ScheduleMail> ScheduleMail { get; set; }

        public DbSet<ScheduleTask> ScheduleTask { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = AppDomain.CurrentDomain.BaseDirectory + "Data\\schedule.db";
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ScheduleMail>().HasKey(t => new { t.Mail, t.Index });
            modelBuilder.Entity<ScheduleTask>().HasKey(t => new { t.Name });

            base.OnModelCreating(modelBuilder);
        }
    }
}
