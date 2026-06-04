using System.Text.Json.Serialization;

namespace LMSDataExtraction.Application.Dtos;

public class CanvasUserDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("primary_email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("enrollments")]
    public IEnumerable<CanvasEnrollmentDto> Enrollments { get; set; } = new List<CanvasEnrollmentDto>();
}
