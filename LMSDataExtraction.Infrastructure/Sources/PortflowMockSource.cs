using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Domain.Entities;
using LMSDataExtraction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LMSDataExtraction.Infrastructure.Sources;

public class PortflowMockSource : IPortflowSource
{
    private readonly AppDbContext _context;

    public PortflowMockSource(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LearningGoal>> GetLearningGoalsAsync()
    {
        return await _context.LearningGoals.ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsAsync()
    {
        return await _context.Reviews.ToListAsync();
    }

    public async Task<IEnumerable<Snapshot>> GetSnapshotsAsync()
    {
        return await _context.Snapshots.ToListAsync();
    }
}
