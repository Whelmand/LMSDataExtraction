namespace LMSDataExtraction.Application.Dtos.Competences;

// A cell in the matrix: per (Layer, HboiActivity) we track which
// level the student has already achieved (achieved, green) and which level
// they want to demonstrate (target, yellow). Both may be null.
public sealed class CompetenceProgressDto
{
    public Guid Id { get; init; }
    public HboiLayer Layer { get; init; }
    public HboiActivity HboiActivity { get; init; }

    public int? AchievedLevel { get; init; }
    public int? TargetLevel { get; init; }

    public string? Explanation { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
