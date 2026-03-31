using WorkService.Models.DTOs;

namespace WorkService.Services
{
    public class ProposalServiceClient
    {
        private readonly HttpClient _httpClient;
        public ProposalServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Dictionary<Guid, List<Guid>>> GetExecutorsByTaskIds(List<Guid> taskIds)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/proposals/by-tasks",
                new { taskIds });

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<GetExecutorsByTasksResponse>();

            return result?.Data ?? new Dictionary<Guid, List<Guid>>();
        }
    }
}
