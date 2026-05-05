namespace LMSDataExtraction.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public int CanvasId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
