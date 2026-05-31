using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSDataExtraction.Domain.Entities;

public class Competence
{
    public int Id { get; set; }
    public int CompetenceToolId { get; set; }
    public int UserId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Level { get; set; } = string.Empty;

    public DateTime? AchievedAt { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}
