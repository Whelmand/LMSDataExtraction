using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSDataExtraction.Domain.Entities;

public class Activity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CourseId { get; set; }

    [Required]
    public string SourceType { get; set; } = string.Empty;

    public int SourceId { get; set; }
    public decimal? Score { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int? AssignmentId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [ForeignKey("CourseId")]
    public Course Course { get; set; } = null!;

    [ForeignKey("AssignmentId")]
    public Assignment? Assignment { get; set; }
}
