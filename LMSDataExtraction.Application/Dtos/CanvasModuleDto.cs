using System.Text.Json.Serialization;

namespace LMSDataExtraction.Application.Dtos;

public class CanvasModuleDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public int Position { get; set; }
}
