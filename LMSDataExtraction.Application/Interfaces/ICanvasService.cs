using LMSDataExtraction.Application.Dtos;

namespace LMSDataExtraction.Application.Interfaces;

public interface ICanvasService
{
    Task<IEnumerable<CanvasCourseDto>> GetCoursesAsync(string token);
    Task<IEnumerable<CanvasModuleDto>> GetModulesAsync(string token, int courseCanvasId);
    Task<IEnumerable<CanvasAssignmentDto>> GetAssignmentsAsync(string token, int courseCanvasId);
    Task<CanvasUserDto> GetCurrentUserAsync(string token);
    Task<IEnumerable<CanvasSubmissionDto>> GetSubmissionsAsync(string token, int courseCanvasId);
}
