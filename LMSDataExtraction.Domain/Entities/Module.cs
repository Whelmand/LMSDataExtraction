namespace LMSDataExtraction.Domain.Entities;

public class Module
{
    public int Id { get; set; }
    public int CanvasId { get; set; }
    public int CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Position { get; set; }

    public Course Course { get; set; } = null!;
}
