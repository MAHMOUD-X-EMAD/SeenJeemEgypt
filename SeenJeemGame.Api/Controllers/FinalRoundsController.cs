using Microsoft.AspNetCore.Mvc;
using SeenJeemGame.Application.Games.FinalRounds;
using SeenJeemGame.Application.Games.FinalRounds.Dtos;

namespace SeenJeemGame.Api.Controllers;

[ApiController]
[Route("api/games/{gameSessionId:guid}/final-round")]
public class FinalRoundsController : ControllerBase
{
    private readonly IFinalRoundService _finalRoundService;

    public FinalRoundsController(
        IFinalRoundService finalRoundService)
    {
        _finalRoundService = finalRoundService;
    }

    [HttpPost("start")]
    public async Task<ActionResult<FinalRoundStateResponse>> StartFinalRound(
        Guid gameSessionId)
    {
        var response = await _finalRoundService
            .StartFinalRoundAsync(gameSessionId);

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<FinalRoundStateResponse>> GetFinalRound(
        Guid gameSessionId)
    {
        var response = await _finalRoundService
            .GetFinalRoundAsync(gameSessionId);

        if (response is null)
            return NotFound("Final round not found.");

        return Ok(response);
    }

    [HttpPost("{finalRoundId:guid}/lock-wagers")]
    public async Task<ActionResult<FinalRoundStateResponse>> LockWagers(
        Guid gameSessionId,
        Guid finalRoundId,
        [FromBody] LockFinalRoundWagersRequest request)
    {
        var response = await _finalRoundService.LockWagersAsync(
            gameSessionId,
            finalRoundId,
            request);

        return Ok(response);
    }

    [HttpPost("{finalRoundId:guid}/reveal-question")]
    public async Task<ActionResult<FinalRoundStateResponse>> RevealQuestion(
        Guid gameSessionId,
        Guid finalRoundId)
    {
        var response = await _finalRoundService.RevealQuestionAsync(
            gameSessionId,
            finalRoundId);

        return Ok(response);
    }

    [HttpPost("{finalRoundId:guid}/reveal-answer")]
    public async Task<ActionResult<FinalRoundStateResponse>> RevealAnswer(
        Guid gameSessionId,
        Guid finalRoundId)
    {
        var response = await _finalRoundService.RevealAnswerAsync(
            gameSessionId,
            finalRoundId);

        return Ok(response);
    }

    [HttpPost("{finalRoundId:guid}/complete")]
    public async Task<ActionResult<FinalRoundStateResponse>> CompleteFinalRound(
        Guid gameSessionId,
        Guid finalRoundId,
        [FromBody] CompleteFinalRoundRequest request)
    {
        var response = await _finalRoundService.CompleteFinalRoundAsync(
            gameSessionId,
            finalRoundId,
            request);

        return Ok(response);
    }
}