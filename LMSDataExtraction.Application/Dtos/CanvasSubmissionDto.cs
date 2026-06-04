using System.Text.Json.Serialization;

namespace LMSDataExtraction.Application.Dtos;

public class CanvasSubmissionDto
{
    [JsonPropertyName("assignment_id")]
    public int AssignmentId { get; set; }

    [JsonPropertyName("score")]
    public decimal? Score { get; set; }

    [JsonPropertyName("submitted_at")]
    public DateTime? SubmittedAt { get; set; }
}
