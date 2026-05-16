using LMSDataExtraction.Application.Dtos;

namespace LMSDataExtraction.Application.Interfaces;

public interface ICanvasService
{
    Task<IEnumerable<CanvasCourseDto>> GetCoursesAsync(string token);
}
