using Microsoft.EntityFrameworkCore;
using SeenJeemGame.Application.Games.FinalRounds;
using SeenJeemGame.Application.Games.FinalRounds.Dtos;
using SeenJeemGame.Domain.Entities;
using SeenJeemGame.Domain.Enums;
using SeenJeemGame.Infrastructure.Persistence;

namespace SeenJeemGame.Infrastructure.Services.Games;

public class FinalRoundService : IFinalRoundService
{
    private readonly AppDbContext _dbContext;

    public FinalRoundService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FinalRoundStateResponse> StartFinalRoundAsync(
        Guid gameSessionId)
    {
        var existingFinalRound = await FinalRoundQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GameSessionId == gameSessionId);

        if (existingFinalRound is not null)
        {
            return MapToResponse(existingFinalRound);
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        var game = await _dbContext.GameSessions
            .FirstOrDefaultAsync(x => x.Id == gameSessionId);

        if (game is null)
            throw new InvalidOperationException("Game session not found.");

        var teams = await _dbContext.Teams
            .Where(x => x.GameSessionId == gameSessionId)
            .OrderBy(x => x.TurnOrder)
            .ToListAsync();

        if (teams.Count != 2)
            throw new InvalidOperationException(
                "Game must have exactly two teams.");

        var gameQuestionsCount = await _dbContext.GameQuestions
            .CountAsync(x => x.GameSessionId == gameSessionId);

        if (gameQuestionsCount == 0)
            throw new InvalidOperationException(
                "Game does not contain any questions.");

        var hasUnusedQuestions = await _dbContext.GameQuestions
            .AnyAsync(x =>
                x.GameSessionId == gameSessionId &&
                !x.IsUsed);

        if (hasUnusedQuestions)
        {
            throw new InvalidOperationException(
                "You cannot start the final round before completing all board questions.");
        }

        var activeQuestionIds = await _dbContext.FinalRoundQuestions
            .Where(x => x.IsActive)
            .Select(x => x.Id)
            .ToListAsync();

        if (activeQuestionIds.Count == 0)
        {
            throw new InvalidOperationException(
                "No active final round questions are available.");
        }

        var selectedQuestionId =
            activeQuestionIds[Random.Shared.Next(activeQuestionIds.Count)];

        var finalRound = new FinalRound
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameSessionId,
            FinalRoundQuestionId = selectedQuestionId,
            Status = FinalRoundStatus.WaitingForWagers,
            TimerSeconds = 60,
            CreatedAt = DateTime.UtcNow
        };

        var teamResults = teams
            .Select(team => new FinalRoundTeamResult
            {
                Id = Guid.NewGuid(),
                FinalRoundId = finalRound.Id,
                TeamId = team.Id,
                Wager = 0,
                ScoreDelta = 0
            })
            .ToList();

        _dbContext.FinalRounds.Add(finalRound);
        _dbContext.FinalRoundTeamResults.AddRange(teamResults);

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        var createdFinalRound = await FinalRoundQuery()
            .AsNoTracking()
            .FirstAsync(x => x.Id == finalRound.Id);

        return MapToResponse(createdFinalRound);
    }

    public async Task<FinalRoundStateResponse?> GetFinalRoundAsync(
        Guid gameSessionId)
    {
        var finalRound = await FinalRoundQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.GameSessionId == gameSessionId);

        return finalRound is null
            ? null
            : MapToResponse(finalRound);
    }

    public async Task<FinalRoundStateResponse> LockWagersAsync(
        Guid gameSessionId,
        Guid finalRoundId,
        LockFinalRoundWagersRequest request)
    {
        if (request.Teams is null || request.Teams.Count != 2)
        {
            throw new InvalidOperationException(
                "You must submit wagers for exactly two teams.");
        }

        if (request.Teams
            .Select(x => x.TeamId)
            .Distinct()
            .Count() != 2)
        {
            throw new InvalidOperationException(
                "Duplicate team ids are not allowed.");
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        var finalRound = await FinalRoundQuery()
            .FirstOrDefaultAsync(x =>
                x.Id == finalRoundId &&
                x.GameSessionId == gameSessionId);

        if (finalRound is null)
            throw new InvalidOperationException("Final round not found.");

        if (finalRound.Status != FinalRoundStatus.WaitingForWagers)
        {
            throw new InvalidOperationException(
                "Wagers can only be locked while waiting for wagers.");
        }

        if (finalRound.TeamResults.Count != 2)
        {
            throw new InvalidOperationException(
                "Final round must contain exactly two team results.");
        }

        foreach (var wagerRequest in request.Teams)
        {
            var result = finalRound.TeamResults
                .FirstOrDefault(x => x.TeamId == wagerRequest.TeamId);

            if (result is null)
            {
                throw new InvalidOperationException(
                    "Team does not belong to this final round.");
            }

            if (result.Team.GameSessionId != gameSessionId)
            {
                throw new InvalidOperationException(
                    "Team does not belong to this game.");
            }

            var maximumWager = Math.Max(result.Team.Score, 0);

            if (wagerRequest.Wager < 0)
            {
                throw new InvalidOperationException(
                    $"Wager cannot be negative for team {result.Team.Name}.");
            }

            if (wagerRequest.Wager > maximumWager)
            {
                throw new InvalidOperationException(
                    $"Wager for team {result.Team.Name} cannot exceed {maximumWager}.");
            }

            result.Wager = wagerRequest.Wager;
            result.WagerLockedAt = DateTime.UtcNow;
        }

        finalRound.Status = FinalRoundStatus.WagersLocked;
        finalRound.WagersLockedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapToResponse(finalRound);
    }

    public async Task<FinalRoundStateResponse> RevealQuestionAsync(
        Guid gameSessionId,
        Guid finalRoundId)
    {
        var finalRound = await FinalRoundQuery()
            .FirstOrDefaultAsync(x =>
                x.Id == finalRoundId &&
                x.GameSessionId == gameSessionId);

        if (finalRound is null)
            throw new InvalidOperationException("Final round not found.");

        if (finalRound.Status != FinalRoundStatus.WagersLocked)
        {
            throw new InvalidOperationException(
                "You must lock the wagers before revealing the question.");
        }

        if (finalRound.TeamResults.Any(x => !x.WagerLockedAt.HasValue))
        {
            throw new InvalidOperationException(
                "Both team wagers must be locked first.");
        }

        finalRound.Status = FinalRoundStatus.QuestionRevealed;
        finalRound.QuestionRevealedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToResponse(finalRound);
    }

    public async Task<FinalRoundStateResponse> RevealAnswerAsync(
        Guid gameSessionId,
        Guid finalRoundId)
    {
        var finalRound = await FinalRoundQuery()
            .FirstOrDefaultAsync(x =>
                x.Id == finalRoundId &&
                x.GameSessionId == gameSessionId);

        if (finalRound is null)
            throw new InvalidOperationException("Final round not found.");

        if (finalRound.Status != FinalRoundStatus.QuestionRevealed)
        {
            throw new InvalidOperationException(
                "You must reveal the question before revealing the answer.");
        }

        finalRound.Status = FinalRoundStatus.AnswerRevealed;
        finalRound.AnswerRevealedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToResponse(finalRound);
    }

    public async Task<FinalRoundStateResponse> CompleteFinalRoundAsync(
        Guid gameSessionId,
        Guid finalRoundId,
        CompleteFinalRoundRequest request)
    {
        if (request.Teams is null || request.Teams.Count != 2)
        {
            throw new InvalidOperationException(
                "You must submit results for exactly two teams.");
        }

        if (request.Teams
            .Select(x => x.TeamId)
            .Distinct()
            .Count() != 2)
        {
            throw new InvalidOperationException(
                "Duplicate team ids are not allowed.");
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        var game = await _dbContext.GameSessions
            .FirstOrDefaultAsync(x => x.Id == gameSessionId);

        if (game is null)
            throw new InvalidOperationException("Game session not found.");

        var finalRound = await FinalRoundQuery()
            .FirstOrDefaultAsync(x =>
                x.Id == finalRoundId &&
                x.GameSessionId == gameSessionId);

        if (finalRound is null)
            throw new InvalidOperationException("Final round not found.");

        if (finalRound.Status == FinalRoundStatus.Completed)
        {
            throw new InvalidOperationException(
                "Final round is already completed.");
        }

        if (finalRound.Status != FinalRoundStatus.AnswerRevealed)
        {
            throw new InvalidOperationException(
                "You must reveal the answer before completing the final round.");
        }

        foreach (var answerRequest in request.Teams)
        {
            var result = finalRound.TeamResults
                .FirstOrDefault(x =>
                    x.TeamId == answerRequest.TeamId);

            if (result is null)
            {
                throw new InvalidOperationException(
                    "Team does not belong to this final round.");
            }

            if (result.Team.GameSessionId != gameSessionId)
            {
                throw new InvalidOperationException(
                    "Team does not belong to this game.");
            }

            result.AnswerText = answerRequest.AnswerText?.Trim();
            result.IsCorrect = answerRequest.IsCorrect;
            result.AnswerSubmittedAt = DateTime.UtcNow;

            result.ScoreDelta = answerRequest.IsCorrect
                ? result.Wager
                : -result.Wager;

            result.Team.Score += result.ScoreDelta;
        }

        finalRound.Status = FinalRoundStatus.Completed;
        finalRound.CompletedAt = DateTime.UtcNow;

        game.CurrentTurnId = null;

        game.Status = GameStatus.Finished;

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapToResponse(finalRound);
    }

    private IQueryable<FinalRound> FinalRoundQuery()
    {
        return _dbContext.FinalRounds
            .Include(x => x.Question)
            .Include(x => x.TeamResults)
                .ThenInclude(x => x.Team);
    }

    private static FinalRoundStateResponse MapToResponse(
        FinalRound finalRound)
    {
        var showQuestion =
            finalRound.Status == FinalRoundStatus.QuestionRevealed ||
            finalRound.Status == FinalRoundStatus.AnswerRevealed ||
            finalRound.Status == FinalRoundStatus.Completed;

        var showAnswer =
            finalRound.Status == FinalRoundStatus.AnswerRevealed ||
            finalRound.Status == FinalRoundStatus.Completed;

        Guid? winnerTeamId = null;
        string? winnerTeamName = null;
        var isDraw = false;

        if (finalRound.Status == FinalRoundStatus.Completed)
        {
            var orderedTeams = finalRound.TeamResults
                .OrderByDescending(x => x.Team.Score)
                .ToList();

            if (orderedTeams.Count == 2)
            {
                if (orderedTeams[0].Team.Score ==
                    orderedTeams[1].Team.Score)
                {
                    isDraw = true;
                }
                else
                {
                    winnerTeamId = orderedTeams[0].TeamId;
                    winnerTeamName = orderedTeams[0].Team.Name;
                }
            }
        }

        return new FinalRoundStateResponse
        {
            FinalRoundId = finalRound.Id,
            GameSessionId = finalRound.GameSessionId,
            Status = finalRound.Status.ToString(),

            CategoryName = finalRound.Question.CategoryName,
            TimerSeconds = finalRound.TimerSeconds,

            QuestionText = showQuestion
                ? finalRound.Question.Text
                : null,

            CorrectAnswer = showAnswer
                ? finalRound.Question.CorrectAnswer
                : null,

            ImageUrl = showQuestion
                ? finalRound.Question.ImageUrl
                : null,

            AudioUrl = showQuestion
                ? finalRound.Question.AudioUrl
                : null,

            CreatedAt = finalRound.CreatedAt,
            WagersLockedAt = finalRound.WagersLockedAt,
            QuestionRevealedAt = finalRound.QuestionRevealedAt,
            AnswerRevealedAt = finalRound.AnswerRevealedAt,
            CompletedAt = finalRound.CompletedAt,

            WinnerTeamId = winnerTeamId,
            WinnerTeamName = winnerTeamName,
            IsDraw = isDraw,

            Teams = finalRound.TeamResults
                .OrderBy(x => x.Team.TurnOrder)
                .Select(result => new FinalRoundTeamStateDto
                {
                    TeamId = result.TeamId,
                    TeamName = result.Team.Name,

                    ScoreBeforeFinalRound =
                        result.Team.Score - result.ScoreDelta,

                    CurrentScore = result.Team.Score,

                    Wager = result.WagerLockedAt.HasValue
                        ? result.Wager
                        : null,

                    IsWagerLocked =
                        result.WagerLockedAt.HasValue,

                    AnswerText = showAnswer
                        ? result.AnswerText
                        : null,

                    IsCorrect = showAnswer
                        ? result.IsCorrect
                        : null,

                    ScoreDelta =
                        finalRound.Status == FinalRoundStatus.Completed
                            ? result.ScoreDelta
                            : 0
                })
                .ToList()
        };
    }
}