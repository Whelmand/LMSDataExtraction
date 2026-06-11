using System.Text.Json.Serialization;

namespace LMSDataExtraction.Application.Dtos;

public class CanvasOutcomeGroupDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("vendor_guid")]
    public string? VendorGuid { get; set; }

    [JsonPropertyName("context_id")]
    public int? ContextId { get; set; }

    [JsonPropertyName("context_type")]
    public string? ContextType { get; set; }

    [JsonPropertyName("parent_outcome_group")]
    public CanvasOutcomeGroupReferenceDto? ParentOutcomeGroup { get; set; }
}

public class CanvasOutcomeGroupReferenceDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}
