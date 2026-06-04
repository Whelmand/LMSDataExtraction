using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMSDataExtraction.Infrastructure.Persistence;

public class ModuleRepository : IModuleRepository
{
    private readonly AppDbContext _context;

    public ModuleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByCanvasIdAsync(int canvasId)
    {
        Module? existingModule = await _context.Modules.FirstOrDefaultAsync(
            module => module.CanvasId == canvasId
        );

        return existingModule != null;
    }

    public async Task SaveAsync(Module module)
    {
        _context.Modules.Add(module);
        await _context.SaveChangesAsync();
    }
}
