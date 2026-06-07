using System.ComponentModel.DataAnnotations;

namespace LMSDataExtraction.Application.Dtos.Competences;

// Upsert of a cell: identified by (Layer, HboiActivity), and sets
// AchievedLevel / TargetLevel. null explicitly means 'clear'.
public sealed class SetCompetenceRequest
{
    [Required]
    public HboiLayer Layer { get; set; }

    [Required]
    public HboiActivity HboiActivity { get; set; }

    [Range(1, 3)]
    public int? AchievedLevel { get; set; }

    [Range(1, 3)]
    public int? TargetLevel { get; set; }

    public string? Explanation { get; set; }
}
