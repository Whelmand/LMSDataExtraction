using LMSDataExtraction.Domain.Entities;

namespace LMSDataExtraction.Application.Interfaces;

public interface IPortflowSource
{
    Task<IEnumerable<LearningGoal>> GetLearningGoalsAsync();
    Task<IEnumerable<Review>> GetReviewsAsync();
    Task<IEnumerable<Snapshot>> GetSnapshotsAsync();
}
