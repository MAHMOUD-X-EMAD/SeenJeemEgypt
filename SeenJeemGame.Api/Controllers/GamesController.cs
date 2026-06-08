using Microsoft.AspNetCore.Mvc;
using SeenJeemGame.Application.Games;
using SeenJeemGame.Application.Games.Dtos;

namespace SeenJeemGame.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameSetupService _gameSetupService;
    private readonly IGamePlayService _gamePlayService;

    public GamesController(
        IGameSetupService gameSetupService,
        IGamePlayService gamePlayService)
    {
        _gameSetupService = gameSetupService;
        _gamePlayService = gamePlayService;
    }

    [HttpPost("setup")]
    public async Task<ActionResult<GameSetupResponse>> CreateGame(CreateGameRequest request)
    {
        try
        {
            var game = await _gameSetupService.CreateGameAsync(request);

            return Ok(game);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{gameSessionId:guid}/board")]
    public async Task<ActionResult<GameSetupResponse>> GetBoard(Guid gameSessionId)
    {
        var game = await _gameSetupService.GetGameBoardAsync(gameSessionId);

        if (game is null)
            return NotFound();

        return Ok(game);
    }

    [HttpPost("{gameSessionId:guid}/select-question")]
    public async Task<ActionResult<SelectedQuestionResponse>> SelectQuestion(
        Guid gameSessionId,
        SelectQuestionRequest request)
    {
        try
        {
            var selectedQuestion = await _gamePlayService.SelectQuestionAsync(
                gameSessionId,
                request);

            return Ok(selectedQuestion);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{gameSessionId:guid}/turns/{gameTurnId:guid}/reveal-answer")]
    public async Task<ActionResult<RevealAnswerResponse>> RevealAnswer(
        Guid gameSessionId,
        Guid gameTurnId)
    {
        try
        {
            var answer = await _gamePlayService.RevealAnswerAsync(
                gameSessionId,
                gameTurnId);

            return Ok(answer);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{gameSessionId:guid}/turns/{gameTurnId:guid}/award-points")]
    public async Task<ActionResult<AwardPointsResponse>> AwardPoints(
    Guid gameSessionId,
    Guid gameTurnId,
    AwardPointsRequest request)
    {
        try
        {
            var result = await _gamePlayService.AwardPointsAsync(
                gameSessionId,
                gameTurnId,
                request);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{gameSessionId:guid}/turns/{gameTurnId:guid}/use-help-option")]
    public async Task<ActionResult<UseHelpOptionResponse>> UseHelpOption(
    Guid gameSessionId,
    Guid gameTurnId,
    UseHelpOptionRequest request)
    {
        try
        {
            var result = await _gamePlayService.UseHelpOptionAsync(
                gameSessionId,
                gameTurnId,
                request);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{gameSessionId:guid}/adjust-score")]
    public async Task<ActionResult<AdjustTeamScoreResponse>> AdjustScore(
    Guid gameSessionId,
    AdjustTeamScoreRequest request)
    {
        try
        {
            var result = await _gamePlayService.AdjustTeamScoreAsync(
                gameSessionId,
                request);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}