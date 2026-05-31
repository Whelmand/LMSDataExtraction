using System.ComponentModel.DataAnnotations;

namespace LMSDataExtraction.Domain.Entities;

// Komt overeen met Portflow RepositoryGoal.
public class LearningGoal
{
    public int Id { get; set; }
    public Guid PortflowUuid { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Nickname { get; set; } = string.Empty;

    [Required]
    public string SourcedId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
