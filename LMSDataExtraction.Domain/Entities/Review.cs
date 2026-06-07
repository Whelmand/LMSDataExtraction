using System.ComponentModel.DataAnnotations;

namespace LMSDataExtraction.Domain.Entities;

// Corresponds to Portflow ProgressReviewResult.
public class Review
{
    public int Id { get; set; }
    public Guid ReviewRequestId { get; set; }
    public Guid ReviewerId { get; set; }

    public decimal Score { get; set; }

    [Required]
    public string ReviewerRole { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; }
}
