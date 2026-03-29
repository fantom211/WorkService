namespace WorkService.Models.DTOs
{
    public class GetExexutorsByTasksRequest
    {
        public List<Guid> TaskIds { get; set; } = new List<Guid>();
    }
}
