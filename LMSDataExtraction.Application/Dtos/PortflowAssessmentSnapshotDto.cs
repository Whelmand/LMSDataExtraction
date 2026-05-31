using System.Text.Json.Serialization;

namespace LMSDataExtraction.Application.Dtos;

public class PortflowAssessmentSnapshotDto
{
    [JsonPropertyName("uuid")]
    public Guid Uuid { get; set; }

    [JsonPropertyName("user_uuid")]
    public Guid UserUuid { get; set; }

    [JsonPropertyName("assessment_url")]
    public string AssessmentUrl { get; set; } = string.Empty;

    [JsonPropertyName("assessment_password")]
    public string AssessmentPassword { get; set; } = string.Empty;

    [JsonPropertyName("lti_submission_count")]
    public int LtiSubmissionCount { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
