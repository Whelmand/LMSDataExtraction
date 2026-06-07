using System.Collections.Concurrent;
using LMSDataExtraction.Application.Dtos.Competences;
using LMSDataExtraction.Application.Interfaces;

namespace LMSDataExtraction.Infrastructure.Sources;

// In-memory mock store for the HBO-i competence tool.
// Registered as a singleton in Program.cs: state persists between
// requests, but disappears when the Web App restarts. No DB.
public class HboiCompetenceMockStore : IHboiCompetenceStore
{
    public const int MinLevel = 1;
    public const int MaxLevel = 3;

    // The 5 architecture layers that use the "main" activities.
    private static readonly HboiLayer[] ArchitectureLayers =
    {
        HboiLayer.UserInteraction,
        HboiLayer.Software,
        HboiLayer.HardwareInterfacing,
        HboiLayer.Infrastructure,
        HboiLayer.OrganisationalProcesses,
    };

    // The 5 main activities (apply to the architecture layers).
    private static readonly HboiActivity[] MainActivities =
    {
        HboiActivity.Analysis,
        HboiActivity.Advise,
        HboiActivity.Design,
        HboiActivity.Realisation,
        HboiActivity.ManageAndControl,
    };

    // The 2 professional-development activities (only for the
    // Professional Development layer).
    private static readonly HboiActivity[] ProfessionalDevelopmentActivities =
    {
        HboiActivity.PersonalLeadership,
        HboiActivity.ProfessionalStandard,
    };

    private static readonly DateTimeOffset SeedTimestamp =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly ConcurrentDictionary<CellKey, CompetenceProgressDto> _state = new();

    public HboiCompetenceMockStore()
    {
        Seed();
    }

    public IReadOnlyList<CompetenceProgressDto> GetAll()
    {
        return _state.Values
            .OrderBy(x => x.Layer)
            .ThenBy(x => x.HboiActivity)
            .ToList();
    }

    public CompetenceFrameworkResponse GetFramework()
    {
        var cells = new List<CompetenceCellDto>();

        // Main matrix: 5 layers x 5 activities.
        foreach (HboiLayer layer in ArchitectureLayers)
        {
            foreach (HboiActivity activity in MainActivities)
            {
                cells.Add(BuildCell(layer, activity));
            }
        }

        // Professional Development: 1 layer x 2 activities.
        foreach (HboiActivity activity in ProfessionalDevelopmentActivities)
        {
            cells.Add(BuildCell(HboiLayer.ProfessionalDevelopment, activity));
        }

        return new CompetenceFrameworkResponse
        {
            MinLevel = MinLevel,
            MaxLevel = MaxLevel,
            Layers = new[]
            {
                HboiLayer.ProfessionalDevelopment,
                HboiLayer.UserInteraction,
                HboiLayer.Software,
                HboiLayer.HardwareInterfacing,
                HboiLayer.Infrastructure,
                HboiLayer.OrganisationalProcesses,
            },
            Activities = MainActivities,
            ProfessionalDevelopmentAreas = ProfessionalDevelopmentActivities,
            Cells = cells,
        };
    }

    public CompetenceProgressDto Upsert(SetCompetenceRequest request)
    {
        if (!IsValidCombination(request.Layer, request.HboiActivity))
        {
            throw new ArgumentException(
                $"Combination '{request.Layer}' + '{request.HboiActivity}' does not exist in the HBO-i framework.");
        }

        CellKey key = new(request.Layer, request.HboiActivity);
        DateTimeOffset now = DateTimeOffset.UtcNow;

        CompetenceProgressDto updated = _state.AddOrUpdate(
            key,
            _ => new CompetenceProgressDto
            {
                Id = Guid.NewGuid(),
                Layer = request.Layer,
                HboiActivity = request.HboiActivity,
                AchievedLevel = request.AchievedLevel,
                TargetLevel = request.TargetLevel,
                Explanation = request.Explanation,
                CreatedAt = now,
                UpdatedAt = now,
            },
            (_, existing) => new CompetenceProgressDto
            {
                Id = existing.Id,
                Layer = existing.Layer,
                HboiActivity = existing.HboiActivity,
                AchievedLevel = request.AchievedLevel,
                TargetLevel = request.TargetLevel,
                Explanation = request.Explanation,
                CreatedAt = existing.CreatedAt,
                UpdatedAt = now,
            });

        return updated;
    }

    private static bool IsValidCombination(HboiLayer layer, HboiActivity activity)
    {
        if (layer == HboiLayer.ProfessionalDevelopment)
        {
            return ProfessionalDevelopmentActivities.Contains(activity);
        }

        return ArchitectureLayers.Contains(layer) && MainActivities.Contains(activity);
    }

    private static CompetenceCellDto BuildCell(HboiLayer layer, HboiActivity activity)
    {
        var levels = new List<CompetenceLevelDto>(MaxLevel);
        for (int level = MinLevel; level <= MaxLevel; level++)
        {
            levels.Add(new CompetenceLevelDto
            {
                Level = level,
                Description = $"{layer} / {activity} — level {level}",
            });
        }

        return new CompetenceCellDto
        {
            Layer = layer,
            HboiActivity = activity,
            MinLevel = MinLevel,
            MaxLevel = MaxLevel,
            Levels = levels,
        };
    }

    // Seed state matches the example from the screenshot of the
    // competence tool: green = AchievedLevel, yellow = TargetLevel.
    private void Seed()
    {
        // Professional Development: PL-2 green, PL-3 yellow ; PS-2 green, PS-3 yellow.
        SeedCell(HboiLayer.ProfessionalDevelopment, HboiActivity.PersonalLeadership, achieved: 2, target: 3);
        SeedCell(HboiLayer.ProfessionalDevelopment, HboiActivity.ProfessionalStandard, achieved: 2, target: 3);

        // User Interaction: U1 yellow for Advise and Realisation.
        SeedCell(HboiLayer.UserInteraction, HboiActivity.Advise, achieved: null, target: 1);
        SeedCell(HboiLayer.UserInteraction, HboiActivity.Realisation, achieved: null, target: 1);

        // Software: S1+S2 green, S3 yellow — across all 5 activities.
        foreach (HboiActivity activity in MainActivities)
        {
            SeedCell(HboiLayer.Software, activity, achieved: 2, target: 3);
        }

        // Hardware Interfacing: H1+H2 green, H3 empty — across all 5 activities.
        foreach (HboiActivity activity in MainActivities)
        {
            SeedCell(HboiLayer.HardwareInterfacing, activity, achieved: 2, target: null);
        }

        // Infrastructure: I1 green for Realisation.
        SeedCell(HboiLayer.Infrastructure, HboiActivity.Realisation, achieved: 1, target: null);

        // Organisational processes stays completely empty.
    }

    private void SeedCell(HboiLayer layer, HboiActivity activity, int? achieved, int? target)
    {
        CellKey key = new(layer, activity);
        _state[key] = new CompetenceProgressDto
        {
            Id = Guid.NewGuid(),
            Layer = layer,
            HboiActivity = activity,
            AchievedLevel = achieved,
            TargetLevel = target,
            Explanation = null,
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp,
        };
    }

    private readonly record struct CellKey(HboiLayer Layer, HboiActivity Activity);
}
