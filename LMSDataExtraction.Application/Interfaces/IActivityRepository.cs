using LMSDataExtraction.Domain.Entities;

namespace LMSDataExtraction.Application.Interfaces;

public interface IActivityRepository
{
    Task<bool> ExistsByUserAndSourceAsync(int userId, int sourceId, string sourceType);
    Task SaveAsync(Activity activity);
}
