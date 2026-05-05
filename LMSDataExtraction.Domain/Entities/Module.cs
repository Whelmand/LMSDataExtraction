using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSDataExtraction.Domain.Entities;

public class Module
{
    public int Id { get; set; }
    public int CanvasId { get; set; }
    public int CourseId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public int Position { get; set; }

    [ForeignKey("CourseId")]
    public Course Course { get; set; } = null!;
}
