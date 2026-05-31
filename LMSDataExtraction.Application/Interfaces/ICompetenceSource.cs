using LMSDataExtraction.Domain.Entities;

namespace LMSDataExtraction.Application.Interfaces;

public interface ICompetenceSource
{
    Task<IEnumerable<Competence>> GetCompetencesAsync();
}
