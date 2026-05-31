using System.ComponentModel.DataAnnotations;

namespace LMSDataExtraction.Domain.Entities;

// Komt overeen met Portflow AssessmentSnapshot.
public class Snapshot
{
    public int Id { get; set; }
    public Guid PortflowUuid { get; set; }
    public Guid UserUuid { get; set; }

    [Required]
    public string AssessmentUrl { get; set; } = string.Empty;

    [Required]
    public string AssessmentPassword { get; set; } = string.Empty;

    public int LtiSubmissionCount { get; set; }

    public DateTime CreatedAt { get; set; }
}
