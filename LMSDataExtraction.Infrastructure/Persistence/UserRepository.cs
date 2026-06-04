using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMSDataExtraction.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByCanvasIdAsync(int canvasId)
    {
        return await _context.Users.FirstOrDefaultAsync(
            user => user.CanvasId == canvasId
        );
    }

    public async Task SaveAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
}
