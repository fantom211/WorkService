using WorkService.Models.Enums;

namespace WorkService.Models.DTOs
{
    public class TaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Budget { get; set; }
        public string Category { get; set; }
        public string Specialization { get; set; } 
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public StatusTask Status { get; set; }
        public List<TechnologyDto> Technologies { get; set; } = new List<TechnologyDto>();
    }
}
