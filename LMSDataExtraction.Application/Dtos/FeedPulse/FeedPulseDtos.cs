namespace LMSDataExtraction.Application.Dtos.FeedPulse;

// Profile info of the student the FeedPulse is about.
public sealed class FeedPulseStudentDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string CoachName { get; init; } = string.Empty;
}

// One checkpoint = a weekly/biweekly feedback moment with the coach.
public sealed class FeedPulseCheckpointDto
{
    public Guid Id { get; init; }
    public int Number { get; init; }
    public string Week { get; init; } = string.Empty;
    public string CoachName { get; init; } = string.Empty;
    public DateOnly Date { get; init; }
    public bool Locked { get; init; }

    public FeedPulseRating? TeacherRating { get; init; }
    public FeedPulseRating? StudentRating { get; init; }

    public string? TeacherComment { get; init; }
    public string? StudentResponse { get; init; }

    public DateTimeOffset? TeacherCommentAt { get; init; }
    public DateTimeOffset? StudentResponseAt { get; init; }
}

// Complete overview for the UI in a single call: student + all
// checkpoints. The frontend can render both the chart (ratings per
// checkpoint) and the timeline (comments) from this.
public sealed class FeedPulseOverviewDto
{
    public FeedPulseStudentDto Student { get; init; } = new();
    public IReadOnlyList<FeedPulseCheckpointDto> Checkpoints { get; init; } = Array.Empty<FeedPulseCheckpointDto>();
}
