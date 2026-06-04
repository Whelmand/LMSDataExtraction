using System.Text.Json.Serialization;

namespace LMSDataExtraction.Application.Dtos;

public class CanvasAssignmentDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("due_at")]
    public DateTime? DueDate { get; set; }

    [JsonPropertyName("points_possible")]
    public decimal? MaxScore { get; set; }
}
