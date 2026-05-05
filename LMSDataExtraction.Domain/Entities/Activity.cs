namespace LMSDataExtraction.Domain.Entities;

public class Activity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CourseId { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public int SourceId { get; set; }
    public decimal? Score { get; set; }
    public DateTime? CompletedAt { get; set; }

    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
