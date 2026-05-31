namespace LMSDataExtraction.Application.Dtos;

public class CourseResponseDto
{
    public int CanvasId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
}
