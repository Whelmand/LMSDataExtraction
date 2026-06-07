namespace LMSDataExtraction.Application.Dtos.Competences;

// Description of a single level within a cell (1..MaxLevel).
public sealed class CompetenceLevelDto
{
    public int Level { get; init; }
    public string Description { get; init; } = string.Empty;
}

// A cell in the framework grid: which (Layer, HboiActivity) exists and
// which levels (1..MaxLevel) are available.
public sealed class CompetenceCellDto
{
    public HboiLayer Layer { get; init; }
    public HboiActivity HboiActivity { get; init; }
    public int MinLevel { get; init; }
    public int MaxLevel { get; init; }
    public IReadOnlyList<CompetenceLevelDto> Levels { get; init; } = Array.Empty<CompetenceLevelDto>();
}

// Full description of the HBO-i framework as the frontend can
// render it. Static (no student state).
public sealed class CompetenceFrameworkResponse
{
    public int MinLevel { get; init; }
    public int MaxLevel { get; init; }

    public IReadOnlyList<HboiLayer> Layers { get; init; } = Array.Empty<HboiLayer>();
    public IReadOnlyList<HboiActivity> Activities { get; init; } = Array.Empty<HboiActivity>();
    public IReadOnlyList<HboiActivity> ProfessionalDevelopmentAreas { get; init; } = Array.Empty<HboiActivity>();
    public IReadOnlyList<CompetenceCellDto> Cells { get; init; } = Array.Empty<CompetenceCellDto>();
}
