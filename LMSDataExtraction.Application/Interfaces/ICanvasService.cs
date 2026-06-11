using LMSDataExtraction.Application.Dtos;

namespace LMSDataExtraction.Application.Interfaces;

public interface ICanvasService
{
    Task<IEnumerable<CanvasCourseDto>> GetCoursesAsync(string token);
    Task<IEnumerable<CanvasModuleDto>> GetModulesAsync(string token, int courseCanvasId);
    Task<IEnumerable<CanvasAssignmentDto>> GetAssignmentsAsync(string token, int courseCanvasId);
    Task<CanvasUserDto> GetCurrentUserAsync(string token);
    Task<IEnumerable<CanvasSubmissionDto>> GetSubmissionsAsync(string token, int courseCanvasId);
    Task<IEnumerable<CanvasOutcomeGroupDto>> GetOutcomeGroupsAsync(string token, int courseCanvasId);
    Task<IEnumerable<CanvasOutcomeDto>> GetOutcomesAsync(string token, int courseCanvasId);
    Task<IEnumerable<CanvasPeerReviewDto>> GetPeerReviewsAsync(string token, int courseCanvasId, int assignmentCanvasId);
    Task<IEnumerable<CanvasAnnouncementDto>> GetAnnouncementsAsync(string token, int courseCanvasId);
    Task<IEnumerable<CanvasGradeDto>> GetGradesAsync(string token, int courseCanvasId);
}
