using System.Text.Json.Serialization;

namespace LMSDataExtraction.Application.Dtos;

public class CanvasPeerReviewDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    [JsonPropertyName("asset_id")]
    public int AssetId { get; set; }

    [JsonPropertyName("asset_type")]
    public string? AssetType { get; set; }

    [JsonPropertyName("assessor_id")]
    public int AssessorId { get; set; }

    [JsonPropertyName("workflow_state")]
    public string? WorkflowState { get; set; }
}
