using System.ComponentModel.DataAnnotations;

namespace LMSDataExtraction.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public int CanvasId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
