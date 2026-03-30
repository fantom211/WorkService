using WorkService.Models.Enums;

namespace WorkService.Models.DTOs
{
    public class MyTasksDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public float Budget { get; set; }
        public string Category { get; set; }
        public string Specialization { get; set; }
        public DateTime? Deadline { get; set; }
        public StatusTask Status { get; set; }
        public List<TechnologyDto> Technologies { get; set; } = new List<TechnologyDto>();
        public List<Guid> Executors { get; set; } = new List<Guid>();
    }
}
