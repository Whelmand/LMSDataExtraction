using LMSDataExtraction.Application.Dtos.FeedPulse;

namespace LMSDataExtraction.Application.Interfaces;

// Mock store for the FeedPulse feedback tool. In-memory, singleton.
// Independent of the existing IFeedPulseSource (Canvas-side Feedback
// entity), since here we need a richer data model that fits
// the chatbot/coach UI.
public interface IFeedPulseFeedbackStore
{
    FeedPulseOverviewDto GetOverview();

    IReadOnlyList<FeedPulseCheckpointDto> GetCheckpoints();

    FeedPulseCheckpointDto? GetCheckpoint(Guid id);

    FeedPulseCheckpointDto? PostStudentResponse(Guid checkpointId, PostStudentResponseRequest request);
}
