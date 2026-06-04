using LMSDataExtraction.Application.Dtos;
using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LMSDataExtraction.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ActivityController : ControllerBase
{
    private const string AuthorizationHeader = "Authorization";
    private const string BearerPrefix = "Bearer ";
    private const string SubmissionSourceType = "Submission";

    private readonly ICanvasService _canvasService;
    private readonly IActivityRepository _activityRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICourseRepository _courseRepository;

    public ActivityController(
        ICanvasService canvasService,
        IActivityRepository activityRepository,
        IUserRepository userRepository,
        ICourseRepository courseRepository)
    {
        _canvasService = canvasService;
        _activityRepository = activityRepository;
        _userRepository = userRepository;
        _courseRepository = courseRepository;
    }

    [HttpGet("{courseCanvasId}")]
    public async Task<IActionResult> GetByCourse(int courseCanvasId)
    {
        string authHeader = Request.Headers[AuthorizationHeader].ToString();
        string token = authHeader.Substring(BearerPrefix.Length).Trim();

        Course? course = await _courseRepository.GetByCanvasIdAsync(courseCanvasId);

        if (course == null)
        {
            return NotFound("Course not found. Fetch courses first via GET /api/v1/Course.");
        }

        CanvasUserDto canvasUser = await _canvasService.GetCurrentUserAsync(token);

        User? existingUser = await _userRepository.GetByCanvasIdAsync(canvasUser.Id);

        if (existingUser == null)
        {
            User newUser = new User();
            newUser.CanvasId = canvasUser.Id;
            newUser.Name = canvasUser.Name;
            newUser.Email = canvasUser.Email;
            newUser.Role = GetRoleFromEnrollments(canvasUser.Enrollments);

            await _userRepository.SaveAsync(newUser);

            existingUser = await _userRepository.GetByCanvasIdAsync(canvasUser.Id);
        }

        if (existingUser == null)
        {
            return StatusCode(500, "Gebruiker kon niet worden opgeslagen.");
        }

        IEnumerable<CanvasSubmissionDto> submissions = await _canvasService.GetSubmissionsAsync(token, courseCanvasId);

        foreach (CanvasSubmissionDto submission in submissions)
        {
            bool alreadyExists = await _activityRepository.ExistsByUserAndSourceAsync(
                existingUser.Id,
                submission.AssignmentId,
                SubmissionSourceType
            );

            if (alreadyExists == false)
            {
                Activity newActivity = new Activity();
                newActivity.UserId = existingUser.Id;
                newActivity.CourseId = course.Id;
                newActivity.SourceType = SubmissionSourceType;
                newActivity.SourceId = submission.AssignmentId;
                newActivity.Score = submission.Score;
                newActivity.CompletedAt = submission.SubmittedAt;

                await _activityRepository.SaveAsync(newActivity);
            }
        }

        return Ok(submissions);
    }

    private string GetRoleFromEnrollments(IEnumerable<CanvasEnrollmentDto> enrollments)
    {
        foreach (CanvasEnrollmentDto enrollment in enrollments)
        {
            if (string.IsNullOrEmpty(enrollment.Type) == false)
            {
                return enrollment.Type;
            }
        }

        return "StudentEnrollment";
    }
}
