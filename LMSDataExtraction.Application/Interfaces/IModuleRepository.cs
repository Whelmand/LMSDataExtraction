using LMSDataExtraction.Domain.Entities;

namespace LMSDataExtraction.Application.Interfaces;

public interface IModuleRepository
{
    Task<bool> ExistsByCanvasIdAsync(int canvasId);
    Task SaveAsync(Module module);
}
