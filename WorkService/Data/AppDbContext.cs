using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using WorkService.Models.Entities;

namespace WorkService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Technology
            builder.Entity<Technology>()
                .Property(t => t.Name)
                .IsRequired();

            //many to many
            builder.Entity<TaskTechnology>()
                .HasKey(tt => new { tt.TaskId, tt.TechnologyId });

            builder.Entity<TaskTechnology>()
                .HasOne(tt => tt.Task)
                .WithMany(t => t.TaskTechnologies)
                .HasForeignKey(tt => tt.TaskId);

            builder.Entity<TaskTechnology>()
                .HasOne(tt => tt.Technology)
                .WithMany(t => t.TaskTechnologies)
                .HasForeignKey(tt => tt.TechnologyId);

            //enum в строку 
            builder.Entity<TaskEntity>()
                .Property(t => t.Status)
                .HasConversion<string>();

            //decimal для БД
            builder.Entity<TaskEntity>()
            .Property(t => t.Budget)
            .HasColumnType("decimal(18,2)");
        }

        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<Technology> Technologies { get; set; }
        public DbSet<TaskTechnology> TaskTechnologies { get; set; }
    }
}
