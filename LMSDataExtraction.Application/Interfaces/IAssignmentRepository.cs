using LMSDataExtraction.Domain.Entities;

namespace LMSDataExtraction.Application.Interfaces;

public interface IAssignmentRepository
{
    Task<bool> ExistsByCanvasIdAsync(int canvasId);
    Task SaveAsync(Assignment assignment);
}
