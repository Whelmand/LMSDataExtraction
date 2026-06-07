using LMSDataExtraction.Application.Dtos.Competences;
using LMSDataExtraction.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LMSDataExtraction.Api.Controllers;

// Endpoints for the HBO-i competence tool of the chatbot frontend.
// Fully mocked: no DB, no external API. State lives in a singleton
// and is reset when the Web App restarts.
[ApiController]
[Route("api/v1/[controller]")]
public class CompetencesController : ControllerBase
{
    private readonly IHboiCompetenceStore _store;

    public CompetencesController(IHboiCompetenceStore store)
    {
        _store = store;
    }

    // GET /api/v1/Competences
    // Returns the current progress per (Layer, HboiActivity).
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CompetenceProgressDto>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<CompetenceProgressDto>> GetAll()
    {
        return Ok(_store.GetAll());
    }

    // GET /api/v1/Competences/framework
    // Static description of the HBO-i framework: which layers,
    // activities, and which levels exist per cell.
    [HttpGet("framework")]
    [ProducesResponseType(typeof(CompetenceFrameworkResponse), StatusCodes.Status200OK)]
    public ActionResult<CompetenceFrameworkResponse> GetFramework()
    {
        return Ok(_store.GetFramework());
    }

    // PUT /api/v1/Competences
    // Upsert of a cell: identified by (Layer, HboiActivity) and sets
    // AchievedLevel / TargetLevel. null = explicitly clear.
    [HttpPut]
    [ProducesResponseType(typeof(CompetenceProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<CompetenceProgressDto> Set([FromBody] SetCompetenceRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            CompetenceProgressDto result = _store.Upsert(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                title: "Invalid combination",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
