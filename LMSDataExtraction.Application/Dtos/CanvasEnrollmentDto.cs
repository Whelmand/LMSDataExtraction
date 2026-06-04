using System.Text.Json.Serialization;

namespace LMSDataExtraction.Application.Dtos;

public class CanvasEnrollmentDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}
