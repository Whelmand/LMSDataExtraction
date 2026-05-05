namespace LMSDataExtraction.Domain.Entities;

public class Assignment
{
    public int Id { get; set; }
    public int CanvasId { get; set; }
    public int CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public decimal? MaxScore { get; set; }

    public Course Course { get; set; } = null!;
}
