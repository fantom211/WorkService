using System.Text.Json.Serialization;
using WorkService.Models.Entities;

namespace WorkService.Models.DTOs
{
    public class NotificationDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("recipient")]
        public Recipient Recipient { get; set; }
    }
}
