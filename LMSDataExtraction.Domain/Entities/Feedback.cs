using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSDataExtraction.Domain.Entities;

public class Feedback
{
    public int Id { get; set; }
    public int FeedPulseId { get; set; }
    public int UserId { get; set; }

    [Required]
    public string Source { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public int? Rating { get; set; }
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}
