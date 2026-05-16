using LMSDataExtraction.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LMSDataExtraction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourseController : ControllerBase
{
    private readonly ICanvasService _canvasService;

    public CourseController(ICanvasService canvasService)
    {
        _canvasService = canvasService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        var token = authHeader.Replace("Bearer ", "");

        var courses = await _canvasService.GetCoursesAsync(token);
        return Ok(courses);
    }
}
