using System.Text.Json.Serialization;

namespace LMSDataExtraction.Application.Dtos;

public class CanvasOutcomeDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("vendor_guid")]
    public string? VendorGuid { get; set; }

    [JsonPropertyName("points_possible")]
    public decimal? PointsPossible { get; set; }

    [JsonPropertyName("mastery_points")]
    public decimal? MasteryPoints { get; set; }

    [JsonPropertyName("context_id")]
    public int? ContextId { get; set; }

    [JsonPropertyName("context_type")]
    public string? ContextType { get; set; }
}
