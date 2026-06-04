using LMSDataExtraction.Application.Dtos;
using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LMSDataExtraction.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AssignmentController : ControllerBase
{
    private const string AuthorizationHeader = "Authorization";
    private const string BearerPrefix = "Bearer ";

    private readonly ICanvasService _canvasService;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ICourseRepository _courseRepository;

    public AssignmentController(ICanvasService canvasService, IAssignmentRepository assignmentRepository, ICourseRepository courseRepository)
    {
        _canvasService = canvasService;
        _assignmentRepository = assignmentRepository;
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

        IEnumerable<CanvasAssignmentDto> canvasAssignments = await _canvasService.GetAssignmentsAsync(token, courseCanvasId);

        foreach (CanvasAssignmentDto canvasAssignment in canvasAssignments)
        {
            bool alreadyExists = await _assignmentRepository.ExistsByCanvasIdAsync(canvasAssignment.Id);

            if (alreadyExists == false)
            {
                Assignment newAssignment = new Assignment();
                newAssignment.CanvasId = canvasAssignment.Id;
                newAssignment.Name = canvasAssignment.Name;
                newAssignment.DueDate = canvasAssignment.DueDate;
                newAssignment.MaxScore = canvasAssignment.MaxScore;
                newAssignment.CourseId = course.Id;

                await _assignmentRepository.SaveAsync(newAssignment);
            }
        }

        return Ok(canvasAssignments);
    }
}
