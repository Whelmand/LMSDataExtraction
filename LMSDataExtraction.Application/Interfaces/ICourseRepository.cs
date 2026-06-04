using LMSDataExtraction.Domain.Entities;

namespace LMSDataExtraction.Application.Interfaces;

public interface ICourseRepository
{
    Task<IEnumerable<Course>> GetAllAsync();
    Task<Course?> GetByIdAsync(int id);
    Task<Course?> GetByCanvasIdAsync(int canvasId);
    Task<bool> ExistsByCanvasIdAsync(int canvasId);
    Task SaveAsync(Course course);
}
