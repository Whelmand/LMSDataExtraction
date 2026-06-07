using LMSDataExtraction.Application.Dtos.FeedPulse;
using LMSDataExtraction.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LMSDataExtraction.Api.Controllers;

// Endpoints for the FeedPulse feedback tool of the chatbot frontend.
// Fully mocked: one fictional student with 9 checkpoints. State lives
// in a singleton and is reset when the Web App restarts.
[ApiController]
[Route("api/v1/[controller]")]
public class FeedPulseController : ControllerBase
{
    private readonly IFeedPulseFeedbackStore _store;

    public FeedPulseController(IFeedPulseFeedbackStore store)
    {
        _store = store;
    }

    // GET /api/v1/FeedPulse/me
    // Full overview (student + all checkpoints) as the FeedPulse page
    // shows it: the frontend can render both the chart and the timeline from this.
    [HttpGet("me")]
    [ProducesResponseType(typeof(FeedPulseOverviewDto), StatusCodes.Status200OK)]
    public ActionResult<FeedPulseOverviewDto> GetOverview()
    {
        return Ok(_store.GetOverview());
    }

    // GET /api/v1/FeedPulse/checkpoints
    // Only the list of checkpoints (useful when you already have the student info).
    [HttpGet("checkpoints")]
    [ProducesResponseType(typeof(IReadOnlyList<FeedPulseCheckpointDto>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<FeedPulseCheckpointDto>> GetCheckpoints()
    {
        return Ok(_store.GetCheckpoints());
    }

    // GET /api/v1/FeedPulse/checkpoints/{id}
    [HttpGet("checkpoints/{id:guid}")]
    [ProducesResponseType(typeof(FeedPulseCheckpointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<FeedPulseCheckpointDto> GetCheckpoint(Guid id)
    {
        FeedPulseCheckpointDto? cp = _store.GetCheckpoint(id);
        return cp is null ? NotFound() : Ok(cp);
    }

    // POST /api/v1/FeedPulse/checkpoints/{id}/response
    // Mock writeback: student posts a response to the coach feedback.
    // Locked checkpoints no longer accept responses.
    [HttpPost("checkpoints/{id:guid}/response")]
    [ProducesResponseType(typeof(FeedPulseCheckpointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<FeedPulseCheckpointDto> PostResponse(
        Guid id,
        [FromBody] PostStudentResponseRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        FeedPulseCheckpointDto? updated = _store.PostStudentResponse(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }
}
