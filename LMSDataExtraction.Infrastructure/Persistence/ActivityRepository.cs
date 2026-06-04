using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMSDataExtraction.Infrastructure.Persistence;

public class ActivityRepository : IActivityRepository
{
    private readonly AppDbContext _context;

    public ActivityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByUserAndSourceAsync(int userId, int sourceId, string sourceType)
    {
        Activity? existingActivity = await _context.Activities.FirstOrDefaultAsync(
            activity => activity.UserId == userId
                     && activity.SourceId == sourceId
                     && activity.SourceType == sourceType
        );

        return existingActivity != null;
    }

    public async Task SaveAsync(Activity activity)
    {
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();
    }
}
