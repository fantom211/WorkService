using WorkService.Models.Enums;

namespace WorkService.Models.DTOs
{
    public class UpdateTaskDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Budget { get; set; }
        public string? Category { get; set; }
        public string? Specialization { get; set; }
        public DateTime? Deadline { get; set; }
        public StatusTask? Status { get; set; }
        public List<string>? Technologies { get; set; }
    }
}
