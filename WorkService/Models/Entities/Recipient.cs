using System.Text.Json.Serialization;

namespace WorkService.Models.Entities
{
    public class Recipient
    {
        [JsonPropertyName("id")]
        public Guid UserId { get; set; }
    }
}
