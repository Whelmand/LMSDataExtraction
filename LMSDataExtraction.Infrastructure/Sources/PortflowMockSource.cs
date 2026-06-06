using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Domain.Entities;

namespace LMSDataExtraction.Infrastructure.Sources;

// In-memory mock voor de Portflow integratie.
// Bewust geen database-afhankelijkheid: deze source is een stub totdat
// de echte Portflow API-integratie geimplementeerd wordt. Hierdoor werkt
// het endpoint overal (lokaal, Docker, Azure) zonder seed-scripts.
public class PortflowMockSource : IPortflowSource
{
    private static readonly DateTime SeedTimestamp =
        new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly IReadOnlyList<LearningGoal> LearningGoals = new List<LearningGoal>
    {
        new()
        {
            Id = 1,
            PortflowUuid = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Software ontwerpen",
            Nickname = "Ontwerp",
            SourcedId = "sis-101",
            CreatedAt = SeedTimestamp,
        },
        new()
        {
            Id = 2,
            PortflowUuid = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Testautomatisering opzetten",
            Nickname = "Testen",
            SourcedId = "sis-102",
            CreatedAt = SeedTimestamp,
        },
        new()
        {
            Id = 3,
            PortflowUuid = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Stakeholders interviewen",
            Nickname = "Interview",
            SourcedId = "sis-103",
            CreatedAt = SeedTimestamp,
        },
    };

    private static readonly IReadOnlyList<Review> Reviews = new List<Review>
    {
        new()
        {
            Id = 1,
            ReviewRequestId = Guid.Parse("aaaaaaa1-0000-0000-0000-000000000001"),
            ReviewerId = Guid.Parse("aaaaaaa2-0000-0000-0000-000000000001"),
            Score = 7.5m,
            ReviewerRole = "student",
            SubmittedAt = SeedTimestamp,
        },
        new()
        {
            Id = 2,
            ReviewRequestId = Guid.Parse("aaaaaaa1-0000-0000-0000-000000000002"),
            ReviewerId = Guid.Parse("aaaaaaa2-0000-0000-0000-000000000002"),
            Score = 8.0m,
            ReviewerRole = "docent",
            SubmittedAt = SeedTimestamp,
        },
    };

    private static readonly IReadOnlyList<Snapshot> Snapshots = new List<Snapshot>
    {
        new()
        {
            Id = 1,
            PortflowUuid = Guid.Parse("bbbbbbb1-0000-0000-0000-000000000001"),
            UserUuid = Guid.Parse("bbbbbbb2-0000-0000-0000-000000000001"),
            AssessmentUrl = "https://assess.example/abc",
            AssessmentPassword = "pw-abc",
            LtiSubmissionCount = 2,
            CreatedAt = SeedTimestamp,
        },
        new()
        {
            Id = 2,
            PortflowUuid = Guid.Parse("bbbbbbb1-0000-0000-0000-000000000002"),
            UserUuid = Guid.Parse("bbbbbbb2-0000-0000-0000-000000000002"),
            AssessmentUrl = "https://assess.example/xyz",
            AssessmentPassword = "pw-xyz",
            LtiSubmissionCount = 5,
            CreatedAt = SeedTimestamp,
        },
    };

    public Task<IEnumerable<LearningGoal>> GetLearningGoalsAsync()
        => Task.FromResult<IEnumerable<LearningGoal>>(LearningGoals);

    public Task<IEnumerable<Review>> GetReviewsAsync()
        => Task.FromResult<IEnumerable<Review>>(Reviews);

    public Task<IEnumerable<Snapshot>> GetSnapshotsAsync()
        => Task.FromResult<IEnumerable<Snapshot>>(Snapshots);
}
