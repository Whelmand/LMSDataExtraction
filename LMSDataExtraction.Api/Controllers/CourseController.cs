using LMSDataExtraction.Application.Dtos;
using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Application.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace LMSDataExtraction.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CourseController : ControllerBase
{
    private const string AuthorizationHeader = "Authorization";
    private const string BearerPrefix = "Bearer ";

    private readonly ICanvasService _canvasService;

    public CourseController(ICanvasService canvasService)
    {
        _canvasService = canvasService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        string authHeader = Request.Headers[AuthorizationHeader].ToString();
        string token = authHeader.Substring(BearerPrefix.Length).Trim();

        IEnumerable<CanvasCourseDto> canvasCourses = await _canvasService.GetCoursesAsync(token);
        IEnumerable<CourseResponseDto> response = CourseMapper.ToResponseList(canvasCourses);

        return Ok(response);
    }
}
