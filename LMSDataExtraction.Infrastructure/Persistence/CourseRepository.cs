using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMSDataExtraction.Infrastructure.Persistence;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;

    public CourseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Course>> GetAllAsync()
    {
        return await _context.Courses.ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        return await _context.Courses.FindAsync(id);
    }

    public async Task<Course?> GetByCanvasIdAsync(int canvasId)
    {
        return await _context.Courses.FirstOrDefaultAsync(
            course => course.CanvasId == canvasId
        );
    }

    public async Task<bool> ExistsByCanvasIdAsync(int canvasId)
    {
        Course? existingCourse = await _context.Courses.FirstOrDefaultAsync(
            course => course.CanvasId == canvasId
        );

        return existingCourse != null;
    }

    public async Task SaveAsync(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
    }
}
