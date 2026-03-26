using System.Text.Json.Serialization;

namespace WorkService.Models.Entities
{
    public class TaskTechnology
    {
        public Guid TaskId { get; set; } 
        public TaskEntity Task { get; set; }

        public Guid TechnologyId { get; set; }
        public Technology Technology { get; set; }
    }
}
