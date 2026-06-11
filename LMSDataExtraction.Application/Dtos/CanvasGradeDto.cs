using System.Text.Json.Serialization;

namespace LMSDataExtraction.Application.Dtos;

public class CanvasGradeDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("assignment_id")]
    public int AssignmentId { get; set; }

    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    [JsonPropertyName("grader_id")]
    public int? GraderId { get; set; }

    [JsonPropertyName("score")]
    public decimal? Score { get; set; }

    [JsonPropertyName("grade")]
    public string? Grade { get; set; }

    [JsonPropertyName("entered_score")]
    public decimal? EnteredScore { get; set; }

    [JsonPropertyName("entered_grade")]
    public string? EnteredGrade { get; set; }

    [JsonPropertyName("graded_at")]
    public DateTime? GradedAt { get; set; }

    [JsonPropertyName("submitted_at")]
    public DateTime? SubmittedAt { get; set; }

    [JsonPropertyName("workflow_state")]
    public string? WorkflowState { get; set; }

    [JsonPropertyName("late")]
    public bool? Late { get; set; }

    [JsonPropertyName("missing")]
    public bool? Missing { get; set; }

    [JsonPropertyName("excused")]
    public bool? Excused { get; set; }
}
