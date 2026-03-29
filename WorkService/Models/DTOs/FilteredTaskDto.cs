using WorkService.Models.Entities;

namespace WorkService.Models.DTOs
{
    public class FilteredTaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public decimal Budget { get; set; }
        public string Specialization { get; set; }
        public List<TechnologyDto> Technologies { get; set; }

    }
}
