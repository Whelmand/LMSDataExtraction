using System.Collections.Concurrent;
using LMSDataExtraction.Application.Dtos.FeedPulse;
using LMSDataExtraction.Application.Interfaces;

namespace LMSDataExtraction.Infrastructure.Sources;

// In-memory mock for the FeedPulse feedback tool.
// Contains a fictional student with 9 checkpoints over a semester.
// Singleton: student responses arriving via POST stay in
// memory until the Web App restarts.
public class FeedPulseFeedbackMockStore : IFeedPulseFeedbackStore
{
    private static readonly FeedPulseStudentDto Student = new()
    {
        Id = Guid.Parse("c1f3a5b7-1111-4d2e-9a00-000000000001"),
        FullName = "Jansen, Mila R.T.",
        DisplayName = "Mila Jansen",
        CoachName = "Bjorn",
    };

    private readonly ConcurrentDictionary<Guid, FeedPulseCheckpointDto> _checkpoints = new();
    private readonly List<Guid> _order = new();

    public FeedPulseFeedbackMockStore()
    {
        Seed();
    }

    public FeedPulseOverviewDto GetOverview()
    {
        return new FeedPulseOverviewDto
        {
            Student = Student,
            Checkpoints = GetCheckpoints(),
        };
    }

    public IReadOnlyList<FeedPulseCheckpointDto> GetCheckpoints()
    {
        return _order
            .Select(id => _checkpoints[id])
            .OrderBy(c => c.Number)
            .ToList();
    }

    public FeedPulseCheckpointDto? GetCheckpoint(Guid id)
    {
        return _checkpoints.TryGetValue(id, out FeedPulseCheckpointDto? cp) ? cp : null;
    }

    public FeedPulseCheckpointDto? PostStudentResponse(Guid checkpointId, PostStudentResponseRequest request)
    {
        if (!_checkpoints.TryGetValue(checkpointId, out FeedPulseCheckpointDto? existing))
        {
            return null;
        }

        if (existing.Locked)
        {
            // Locked checkpoints no longer accept responses.
            return existing;
        }

        // FeedPulseCheckpointDto is a class (not a record), so we copy
        // all fields explicitly and only adjust the student response.
        FeedPulseCheckpointDto updated = new()
        {
            Id = existing.Id,
            Number = existing.Number,
            Week = existing.Week,
            CoachName = existing.CoachName,
            Date = existing.Date,
            Locked = existing.Locked,
            TeacherRating = existing.TeacherRating,
            StudentRating = request.Rating ?? existing.StudentRating,
            TeacherComment = existing.TeacherComment,
            StudentResponse = request.Response,
            TeacherCommentAt = existing.TeacherCommentAt,
            StudentResponseAt = DateTimeOffset.UtcNow,
        };

        _checkpoints[checkpointId] = updated;
        return updated;
    }

    private void Seed()
    {
        // 9 checkpoints across a fictional semester (Feb - Jun 2026).
        // Storyline: calm start, dip around DB redesign, then recovery.
        AddCheckpoint(
            number: 1,
            week: "wk7",
            date: new DateOnly(2026, 2, 16),
            teacherRating: FeedPulseRating.Happy,
            studentRating: FeedPulseRating.Happy,
            teacherComment:
                "Goede start van het semester. De scope-afbakening op het kickoff-document is duidelijk. " +
                "Tip: zorg dat je risicoparagraaf concrete mitigaties bevat in plaats van alleen risico's.",
            studentResponse:
                "Bedankt. Ik werk de mitigaties nog even uit en koppel ze aan de planning.",
            daysAgoComment: 110,
            daysAgoResponse: 109,
            locked: true);

        AddCheckpoint(
            number: 2,
            week: "wk8",
            date: new DateOnly(2026, 2, 23),
            teacherRating: FeedPulseRating.Happy,
            studentRating: FeedPulseRating.Neutral,
            teacherComment:
                "Het domeinmodel met de klassendiagrammen is overzichtelijk. " +
                "Let bij de aggregates op de richting van de relaties — een paar pijlen wijzen nu de verkeerde kant op.",
            studentResponse:
                "Pijlen verbeterd in de laatste versie van het document, ook de cardinaliteiten nog eens nagelopen.",
            daysAgoComment: 103,
            daysAgoResponse: 102,
            locked: true);

        AddCheckpoint(
            number: 3,
            week: "wk9",
            date: new DateOnly(2026, 3, 2),
            teacherRating: FeedPulseRating.Neutral,
            studentRating: FeedPulseRating.Neutral,
            teacherComment:
                "CI-pipeline staat technisch goed in elkaar. De build is groen en de tests draaien. " +
                "Wat ik mis is een korte uitleg in de README waarom je voor GitHub Actions hebt gekozen.",
            studentResponse:
                "Korte motivatie toegevoegd aan README, plus een diagram van de pipeline-stappen.",
            daysAgoComment: 96,
            daysAgoResponse: 95,
            locked: true);

        AddCheckpoint(
            number: 4,
            week: "wk10",
            date: new DateOnly(2026, 3, 9),
            teacherRating: FeedPulseRating.Neutral,
            studentRating: FeedPulseRating.Neutral,
            teacherComment:
                "De code reviews die je deze week op de PR's van je teamgenoten hebt gegeven zijn inhoudelijk. " +
                "Voor jezelf: probeer eens een vaste review-checklist te hanteren, dat scheelt tijd.",
            studentResponse:
                "Checklist opgesteld op basis van de Google engineering practices, scheelt inderdaad.",
            daysAgoComment: 89,
            daysAgoResponse: 88,
            locked: true);

        AddCheckpoint(
            number: 5,
            week: "wk11",
            date: new DateOnly(2026, 3, 16),
            teacherRating: FeedPulseRating.Sad,
            studentRating: FeedPulseRating.Neutral,
            teacherComment:
                "Het databaseschema is in deze sprint twee keer fundamenteel omgegooid zonder migration-strategie. " +
                "Dat is voor het team kostbaar. Maak voor volgende keer eerst een ontwerpvoorstel voordat je merged.",
            studentResponse:
                "Begrepen. Ik heb een korte ADR-template in de repo gezet en gebruik die nu voor schema-wijzigingen.",
            daysAgoComment: 82,
            daysAgoResponse: 81,
            locked: true);

        AddCheckpoint(
            number: 6,
            week: "wk12",
            date: new DateOnly(2026, 3, 23),
            teacherRating: FeedPulseRating.Neutral,
            studentRating: FeedPulseRating.Neutral,
            teacherComment:
                "Authenticatie met JWT werkt netjes; refresh-flow is een aandachtspunt. " +
                "Bekijk hoe je tokens gaat invalideren bij een uitlog-actie — nu blijven ze bruikbaar tot ze verlopen.",
            studentResponse:
                "Refresh-token rotation + denylist toegevoegd, getest met Postman scenarios.",
            daysAgoComment: 75,
            daysAgoResponse: 74,
            locked: true);

        AddCheckpoint(
            number: 7,
            week: "wk13",
            date: new DateOnly(2026, 3, 30),
            teacherRating: FeedPulseRating.Happy,
            studentRating: FeedPulseRating.Happy,
            teacherComment:
                "Mooie sprong in testdekking deze sprint. DI-setup is consistent doorgevoerd in alle services. " +
                "Volgende stap: een paar integratietests bovenop de unit tests, zodat je end-to-end ook gedekt bent.",
            studentResponse:
                "Twee integratietests opgenomen rondom de auth- en feedback-endpoints, draaien in CI.",
            daysAgoComment: 68,
            daysAgoResponse: 67,
            locked: true);

        AddCheckpoint(
            number: 8,
            week: "wk22",
            date: new DateOnly(2026, 5, 25),
            teacherRating: FeedPulseRating.Neutral,
            studentRating: null,
            teacherComment:
                "Swagger-documentatie is grotendeels op orde. Voeg per endpoint nog een voorbeeldresponse toe, " +
                "dan kunnen frontend-collega's zonder vragen aan de slag. Lock blijft op deze checkpoint tot na de demo.",
            studentResponse: null,
            daysAgoComment: 13,
            daysAgoResponse: null,
            locked: true);

        AddCheckpoint(
            number: 9,
            week: "wk23",
            date: new DateOnly(2026, 6, 1),
            teacherRating: FeedPulseRating.Happy,
            studentRating: null,
            teacherComment:
                "Eindreflectie ziet er volwassen uit. Je benoemt expliciet wat je geleerd hebt van de schema-incident in wk11 " +
                "en hoe dat je werkwijze rond ontwerp-beslissingen heeft veranderd. Mooie afsluiting van het semester.",
            studentResponse: null,
            daysAgoComment: 0,
            daysAgoResponse: null,
            locked: false);
    }

    private void AddCheckpoint(
        int number,
        string week,
        DateOnly date,
        FeedPulseRating? teacherRating,
        FeedPulseRating? studentRating,
        string? teacherComment,
        string? studentResponse,
        int daysAgoComment,
        int? daysAgoResponse,
        bool locked)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Guid id = Guid.Parse($"feedba11-0000-0000-0000-{number:D12}");

        FeedPulseCheckpointDto cp = new()
        {
            Id = id,
            Number = number,
            Week = week,
            CoachName = Student.CoachName,
            Date = date,
            Locked = locked,
            TeacherRating = teacherRating,
            StudentRating = studentRating,
            TeacherComment = teacherComment,
            StudentResponse = studentResponse,
            TeacherCommentAt = teacherComment is null ? null : now.AddDays(-daysAgoComment),
            StudentResponseAt = studentResponse is null || daysAgoResponse is null
                ? null
                : now.AddDays(-daysAgoResponse.Value),
        };

        _checkpoints[id] = cp;
        _order.Add(id);
    }
}
