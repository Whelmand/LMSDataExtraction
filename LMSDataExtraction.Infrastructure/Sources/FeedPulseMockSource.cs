using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Domain.Entities;
using LMSDataExtraction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LMSDataExtraction.Infrastructure.Sources;

public class FeedPulseMockSource : IFeedPulseSource
{
    private readonly AppDbContext _context;

    public FeedPulseMockSource(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Feedback>> GetFeedbackAsync()
    {
        return await _context.Feedback.ToListAsync();
    }
}
