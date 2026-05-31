using LMSDataExtraction.Domain.Entities;

namespace LMSDataExtraction.Application.Interfaces;

public interface IFeedPulseSource
{
    Task<IEnumerable<Feedback>> GetFeedbackAsync();
}
