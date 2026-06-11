using LMSDataExtraction.Application.Dtos;
using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LMSDataExtraction.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class OutcomeController : ControllerBase
{
    private const string AuthorizationHeader = "Authorization";
    private const string BearerPrefix = "Bearer ";

    private readonly ICanvasService _canvasService;
    private readonly ICourseRepository _courseRepository;

    public OutcomeController(ICanvasService canvasService, ICourseRepository courseRepository)
    {
        _canvasService = canvasService;
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

        IEnumerable<CanvasOutcomeDto> outcomes = await _canvasService.GetOutcomesAsync(token, courseCanvasId);

        return Ok(outcomes);
    }

    [HttpGet("{courseCanvasId}/groups")]
    public async Task<IActionResult> GetGroupsByCourse(int courseCanvasId)
    {
        string authHeader = Request.Headers[AuthorizationHeader].ToString();
        string token = authHeader.Substring(BearerPrefix.Length).Trim();

        Course? course = await _courseRepository.GetByCanvasIdAsync(courseCanvasId);

        if (course == null)
        {
            return NotFound("Course not found. Fetch courses first via GET /api/v1/Course.");
        }

        IEnumerable<CanvasOutcomeGroupDto> groups = await _canvasService.GetOutcomeGroupsAsync(token, courseCanvasId);

        return Ok(groups);
    }
}
