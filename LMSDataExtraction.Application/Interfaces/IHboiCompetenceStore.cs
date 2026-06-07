using LMSDataExtraction.Application.Dtos.Competences;

namespace LMSDataExtraction.Application.Interfaces;

// Mock store for the HBO-i competence tool. Fully in-memory,
// thread-safe, seeded with an example state. No DB dependency.
public interface IHboiCompetenceStore
{
    IReadOnlyList<CompetenceProgressDto> GetAll();

    CompetenceFrameworkResponse GetFramework();

    CompetenceProgressDto Upsert(SetCompetenceRequest request);
}
