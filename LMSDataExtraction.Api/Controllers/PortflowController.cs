using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LMSDataExtraction.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PortflowController : ControllerBase
{
    private readonly IPortflowSource _portflowSource;

    public PortflowController(IPortflowSource portflowSource)
    {
        _portflowSource = portflowSource;
    }

    [HttpGet("learninggoals")]
    public async Task<IActionResult> GetLearningGoals()
    {
        IEnumerable<LearningGoal> learningGoals = await _portflowSource.GetLearningGoalsAsync();
        return Ok(learningGoals);
    }
}
