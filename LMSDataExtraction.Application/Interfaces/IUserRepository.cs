using LMSDataExtraction.Domain.Entities;

namespace LMSDataExtraction.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByCanvasIdAsync(int canvasId);
    Task SaveAsync(User user);
}
