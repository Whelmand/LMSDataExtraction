using System.ComponentModel.DataAnnotations;

namespace LMSDataExtraction.Application.Dtos.FeedPulse;

// Body for posting a student response to a checkpoint.
public sealed class PostStudentResponseRequest
{
    [Required, MinLength(1), MaxLength(4000)]
    public string Response { get; set; } = string.Empty;

    public FeedPulseRating? Rating { get; set; }
}
