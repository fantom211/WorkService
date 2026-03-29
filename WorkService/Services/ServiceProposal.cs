using WorkService.Models.DTOs;

namespace WorkService.Services
{
    public class ServiceProposal
    {
        private readonly HttpClient _httpClient;
        private const string Url = "http://localhost:5211/api/proposals/by-tasks";

        public ServiceProposal(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Dictionary<Guid, List<Guid>>> GetExecutorsByTaskIds(List<Guid> taskIds)
        {
            var response = await _httpClient.PostAsJsonAsync(
                Url,
                new { taskIds });

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<GetExecutorsByTasksResponse>();

            return result?.Data ?? new Dictionary<Guid, List<Guid>>();
        }
    }
}
