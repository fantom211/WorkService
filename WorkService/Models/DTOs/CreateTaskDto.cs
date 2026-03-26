namespace WorkService.Models.DTOs
{
    public class CreateTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; } 
        public decimal Budget { get; set; }
        public string Category { get; set; }
        public DateTime? Deadline { get; set; }
        public string Specialization { get; set; }
        public List<string> Technologies { get; set; } = new List<string>();

    }
}
