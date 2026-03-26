namespace WorkService.Models.Entities
{
    public class Technology
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public ICollection<TaskTechnology> TaskTechnologies { get; set; } = new List<TaskTechnology>();
    }
}
