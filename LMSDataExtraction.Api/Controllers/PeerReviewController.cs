using LMSDataExtraction.Application.Dtos;
using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LMSDataExtraction.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PeerReviewController : ControllerBase
{
    private const string AuthorizationHeader = "Authorization";
    private const string BearerPrefix = "Bearer ";

    private readonly ICanvasService _canvasService;
    private readonly ICourseRepository _courseRepository;

    public PeerReviewController(ICanvasService canvasService, ICourseRepository courseRepository)
    {
        _canvasService = canvasService;
        _courseRepository = courseRepository;
    }

    [HttpGet("{courseCanvasId}/{assignmentCanvasId}")]
    public async Task<IActionResult> GetByAssignment(int courseCanvasId, int assignmentCanvasId)
    {
        string authHeader = Request.Headers[AuthorizationHeader].ToString();
        string token = authHeader.Substring(BearerPrefix.Length).Trim();

        Course? course = await _courseRepository.GetByCanvasIdAsync(courseCanvasId);

        if (course == null)
        {
            return NotFound("Course not found. Fetch courses first via GET /api/v1/Course.");
        }

        IEnumerable<CanvasPeerReviewDto> peerReviews = await _canvasService.GetPeerReviewsAsync(token, courseCanvasId, assignmentCanvasId);

        return Ok(peerReviews);
    }
}
