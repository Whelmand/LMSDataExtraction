using System.Text.Json.Serialization;

namespace LMSDataExtraction.Application.Dtos;

public class PortflowRepositoryGoalDto
{
    [JsonPropertyName("uuid")]
    public Guid Uuid { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; } = string.Empty;

    [JsonPropertyName("sourced_id")]
    public string SourcedId { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
