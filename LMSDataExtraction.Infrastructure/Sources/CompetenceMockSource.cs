using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Domain.Entities;
using LMSDataExtraction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LMSDataExtraction.Infrastructure.Sources;

public class CompetenceMockSource : ICompetenceSource
{
    private readonly AppDbContext _context;

    public CompetenceMockSource(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Competence>> GetCompetencesAsync()
    {
        return await _context.Competences.ToListAsync();
    }
}
