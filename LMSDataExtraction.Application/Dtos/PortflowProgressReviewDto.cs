using System.Text.Json.Serialization;

namespace LMSDataExtraction.Application.Dtos;

public class PortflowProgressReviewDto
{
    [JsonPropertyName("review_request_id")]
    public Guid ReviewRequestId { get; set; }

    [JsonPropertyName("reviewer_id")]
    public Guid ReviewerId { get; set; }

    [JsonPropertyName("score")]
    public decimal Score { get; set; }

    [JsonPropertyName("reviewer_role")]
    public string ReviewerRole { get; set; } = string.Empty;

    [JsonPropertyName("submitted_at")]
    public DateTime SubmittedAt { get; set; }
}
