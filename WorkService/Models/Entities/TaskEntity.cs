using WorkService.Models.Enums;

namespace WorkService.Models.Entities
{
    public class TaskEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; } 
        public string Description { get; set; }
        public decimal Budget { get; set; }
        public string Category { get; set; }
        public string Specialization { get; set; }

        public Guid CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? Deadline { get; set; } = null;

        public StatusTask Status { get; set; } = StatusTask.Open;

        public ICollection<TaskTechnology> TaskTechnologies { get; set; } = new List<TaskTechnology>();

    }
}
