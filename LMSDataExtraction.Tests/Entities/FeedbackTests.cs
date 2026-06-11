using LMSDataExtraction.Domain.Entities;
using Xunit;

namespace LMSDataExtraction.Tests.Entities;

public class FeedbackTests
{
    [Fact]
    public void NewFeedback_ShouldHaveEmptyDefaults()
    {
        Feedback feedback = new Feedback();

        Assert.Equal(0, feedback.Id);
        Assert.Equal(0, feedback.FeedPulseId);
        Assert.Equal(0, feedback.UserId);
        Assert.Equal(string.Empty, feedback.Source);
        Assert.Equal(string.Empty, feedback.Content);
        Assert.Null(feedback.Rating);
        Assert.Null(feedback.CreatedAt);
    }

    [Fact]
    public void AssignedValues_ShouldBeReadBackCorrectly()
    {
        DateTime created = new DateTime(2026, 3, 16, 10, 30, 0);

        Feedback feedback = new Feedback();
        feedback.Source = "FeedPulse";
        feedback.Content = "Goede sprint";
        feedback.Rating = 3;
        feedback.CreatedAt = created;

        Assert.Equal("FeedPulse", feedback.Source);
        Assert.Equal("Goede sprint", feedback.Content);
        Assert.Equal(3, feedback.Rating);
        Assert.Equal(created, feedback.CreatedAt);
    }
}
