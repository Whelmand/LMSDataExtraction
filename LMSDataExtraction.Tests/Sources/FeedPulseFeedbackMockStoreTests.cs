using LMSDataExtraction.Application.Dtos.FeedPulse;
using LMSDataExtraction.Infrastructure.Sources;
using Xunit;

namespace LMSDataExtraction.Tests.Sources;

public class FeedPulseFeedbackMockStoreTests
{
    [Fact]
    public void GetOverview_ShouldReturnStudentAndNineCheckpoints()
    {
        FeedPulseFeedbackMockStore store = new FeedPulseFeedbackMockStore();

        FeedPulseOverviewDto overview = store.GetOverview();

        Assert.Equal("Mila Jansen", overview.Student.DisplayName);
        Assert.Equal("Bjorn", overview.Student.CoachName);
        Assert.Equal(9, overview.Checkpoints.Count);
    }

    [Fact]
    public void GetCheckpoints_ShouldBeOrderedByNumber()
    {
        FeedPulseFeedbackMockStore store = new FeedPulseFeedbackMockStore();

        IReadOnlyList<FeedPulseCheckpointDto> checkpoints = store.GetCheckpoints();

        for (int i = 0; i < checkpoints.Count; i++)
        {
            Assert.Equal(i + 1, checkpoints[i].Number);
        }
    }

    [Fact]
    public void GetCheckpoint_WithUnknownId_ShouldReturnNull()
    {
        FeedPulseFeedbackMockStore store = new FeedPulseFeedbackMockStore();

        FeedPulseCheckpointDto? result = store.GetCheckpoint(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void PostStudentResponse_OnUnlockedCheckpoint_ShouldStoreResponse()
    {
        FeedPulseFeedbackMockStore store = new FeedPulseFeedbackMockStore();
        FeedPulseCheckpointDto unlocked = store.GetCheckpoints().Single(c => !c.Locked);

        PostStudentResponseRequest request = new PostStudentResponseRequest
        {
            Response = "Bedankt voor de feedback.",
            Rating = FeedPulseRating.Happy,
        };

        FeedPulseCheckpointDto? updated = store.PostStudentResponse(unlocked.Id, request);

        Assert.NotNull(updated);
        Assert.Equal("Bedankt voor de feedback.", updated!.StudentResponse);
        Assert.Equal(FeedPulseRating.Happy, updated.StudentRating);
        Assert.NotNull(updated.StudentResponseAt);

        FeedPulseCheckpointDto? reread = store.GetCheckpoint(unlocked.Id);
        Assert.Equal("Bedankt voor de feedback.", reread!.StudentResponse);
    }

    [Fact]
    public void PostStudentResponse_OnLockedCheckpoint_ShouldNotChangeResponse()
    {
        FeedPulseFeedbackMockStore store = new FeedPulseFeedbackMockStore();
        FeedPulseCheckpointDto locked = store.GetCheckpoints().First(c => c.Locked);
        string? originalResponse = locked.StudentResponse;

        PostStudentResponseRequest request = new PostStudentResponseRequest
        {
            Response = "Poging tot wijziging.",
            Rating = FeedPulseRating.Sad,
        };

        FeedPulseCheckpointDto? result = store.PostStudentResponse(locked.Id, request);

        Assert.NotNull(result);
        Assert.Equal(originalResponse, result!.StudentResponse);

        FeedPulseCheckpointDto? reread = store.GetCheckpoint(locked.Id);
        Assert.Equal(originalResponse, reread!.StudentResponse);
    }

    [Fact]
    public void PostStudentResponse_WithUnknownId_ShouldReturnNull()
    {
        FeedPulseFeedbackMockStore store = new FeedPulseFeedbackMockStore();

        PostStudentResponseRequest request = new PostStudentResponseRequest
        {
            Response = "Tekst",
        };

        FeedPulseCheckpointDto? result = store.PostStudentResponse(Guid.NewGuid(), request);

        Assert.Null(result);
    }
}
