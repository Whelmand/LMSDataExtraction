using System.ComponentModel.DataAnnotations;

namespace LMSDataExtraction.Domain.Entities;

public class Course
{
    public int Id { get; set; }
    public int CanvasId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ICollection<Module> Modules { get; set; } = new List<Module>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
