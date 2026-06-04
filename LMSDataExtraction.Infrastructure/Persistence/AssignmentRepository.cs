using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMSDataExtraction.Infrastructure.Persistence;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly AppDbContext _context;

    public AssignmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByCanvasIdAsync(int canvasId)
    {
        Assignment? existingAssignment = await _context.Assignments.FirstOrDefaultAsync(
            assignment => assignment.CanvasId == canvasId
        );

        return existingAssignment != null;
    }

    public async Task SaveAsync(Assignment assignment)
    {
        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();
    }
}
