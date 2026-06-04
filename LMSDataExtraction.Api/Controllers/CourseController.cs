using LMSDataExtraction.Application.Dtos;
using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Application.Mapping;
using LMSDataExtraction.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LMSDataExtraction.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CourseController : ControllerBase
{
    private const string AuthorizationHeader = "Authorization";
    private const string BearerPrefix = "Bearer ";

    private readonly ICanvasService _canvasService;
    private readonly ICourseRepository _courseRepository;

    public CourseController(ICanvasService canvasService, ICourseRepository courseRepository)
    {
        _canvasService = canvasService;
        _courseRepository = courseRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        string authHeader = Request.Headers[AuthorizationHeader].ToString();
        string token = authHeader.Substring(BearerPrefix.Length).Trim();

        IEnumerable<CanvasCourseDto> canvasCourses = await _canvasService.GetCoursesAsync(token);

        foreach (CanvasCourseDto canvasCourse in canvasCourses)
        {
            bool alreadyExists = await _courseRepository.ExistsByCanvasIdAsync(canvasCourse.Id);

            if (alreadyExists == false)
            {
                Course newCourse = new Course();
                newCourse.CanvasId = canvasCourse.Id;
                newCourse.Name = canvasCourse.Name;
                newCourse.Description = canvasCourse.CourseCode;

                await _courseRepository.SaveAsync(newCourse);
            }
        }

        IEnumerable<CourseResponseDto> response = CourseMapper.ToResponseList(canvasCourses);

        return Ok(response);
    }
}
