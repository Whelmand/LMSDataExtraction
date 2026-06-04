using LMSDataExtraction.Application.Dtos;
using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LMSDataExtraction.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ModuleController : ControllerBase
{
    private const string AuthorizationHeader = "Authorization";
    private const string BearerPrefix = "Bearer ";

    private readonly ICanvasService _canvasService;
    private readonly IModuleRepository _moduleRepository;
    private readonly ICourseRepository _courseRepository;

    public ModuleController(ICanvasService canvasService, IModuleRepository moduleRepository, ICourseRepository courseRepository)
    {
        _canvasService = canvasService;
        _moduleRepository = moduleRepository;
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

        IEnumerable<CanvasModuleDto> canvasModules = await _canvasService.GetModulesAsync(token, courseCanvasId);

        foreach (CanvasModuleDto canvasModule in canvasModules)
        {
            bool alreadyExists = await _moduleRepository.ExistsByCanvasIdAsync(canvasModule.Id);

            if (alreadyExists == false)
            {
                Module newModule = new Module();
                newModule.CanvasId = canvasModule.Id;
                newModule.Name = canvasModule.Name;
                newModule.Position = canvasModule.Position;
                newModule.CourseId = course.Id;

                await _moduleRepository.SaveAsync(newModule);
            }
        }

        return Ok(canvasModules);
    }
}
