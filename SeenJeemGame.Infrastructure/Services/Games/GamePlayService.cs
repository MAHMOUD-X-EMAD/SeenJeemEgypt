using Microsoft.EntityFrameworkCore;
using SeenJeemGame.Application.Games;
using SeenJeemGame.Application.Games.Dtos;
using SeenJeemGame.Domain.Entities;
using SeenJeemGame.Domain.Enums;
using SeenJeemGame.Infrastructure.Persistence;

namespace SeenJeemGame.Infrastructure.Services.Games;

public class GamePlayService : IGamePlayService
{
    private readonly AppDbContext _dbContext;

    public GamePlayService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SelectedQuestionResponse> SelectQuestionAsync(
        Guid gameSessionId,
        SelectQuestionRequest request)
    {
        if (request.GameQuestionId == Guid.Empty)
            throw new InvalidOperationException("Game question id is required.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var game = await _dbContext.GameSessions
            .FirstOrDefaultAsync(x => x.Id == gameSessionId);

        if (game is null)
            throw new InvalidOperationException("Game session not found.");

        if (game.Status == GameStatus.Finished || game.Status == GameStatus.Cancelled)
            throw new InvalidOperationException("Game is already finished or cancelled.");

        if (game.CurrentTurnId.HasValue)
        {
            var currentTurnIsStillActive = await _dbContext.GameTurns
                .AnyAsync(x =>
                    x.Id == game.CurrentTurnId.Value &&
                    x.Status != TurnStatus.Completed);

            if (currentTurnIsStillActive)
                throw new InvalidOperationException("There is already an active question.");
        }

        var gameQuestion = await _dbContext.GameQuestions
            .Include(x => x.Question)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x =>
                x.Id == request.GameQuestionId &&
                x.GameSessionId == gameSessionId);

        if (gameQuestion is null)
            throw new InvalidOperationException("Game question not found.");

        if (gameQuestion.IsUsed)
            throw new InvalidOperationException("This question has already been used.");

        var teams = await _dbContext.Teams
            .Where(x => x.GameSessionId == gameSessionId)
            .OrderBy(x => x.TurnOrder)
            .ToListAsync();

        if (teams.Count != 2)
            throw new InvalidOperationException("Game must have exactly two teams.");

        var completedTurnsCount = await _dbContext.GameTurns
            .CountAsync(x =>
                x.GameSessionId == gameSessionId &&
                x.Status == TurnStatus.Completed);

        var mainTeam = completedTurnsCount % 2 == 0
            ? teams.First(x => x.TurnOrder == 1)
            : teams.First(x => x.TurnOrder == 2);

        var secondTeam = teams.First(x => x.Id != mainTeam.Id);
        var isDoublePointsUsed = false;

        if (request.UseDoublePoints)
        {
            var doublePointsOption = await _dbContext.TeamHelpOptions
                .FirstOrDefaultAsync(x =>
                    x.TeamId == mainTeam.Id &&
                    x.Type == HelpOptionType.DoublePoints);

            if (doublePointsOption is null)
                throw new InvalidOperationException("Double points help option is not available for this team.");

            if (doublePointsOption.IsUsed)
                throw new InvalidOperationException("Double points help option has already been used.");

            doublePointsOption.IsUsed = true;
            doublePointsOption.UsedAt = DateTime.UtcNow;

            isDoublePointsUsed = true;
        }

        var basePoints = gameQuestion.Points;
        var finalPoints = isDoublePointsUsed ? basePoints * 2 : basePoints;

        var gameTurn = new GameTurn
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameSessionId,
            GameQuestionId = gameQuestion.Id,
            MainTeamId = mainTeam.Id,
            SecondTeamId = secondTeam.Id,
            Status = TurnStatus.MainTeamAnswering,
            IsDoublePointsUsed = isDoublePointsUsed,
            BasePoints = basePoints,
            FinalPoints = finalPoints,
            CreatedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow
        };

        gameQuestion.IsUsed = true;
        gameQuestion.UsedByTeamId = mainTeam.Id;
        gameQuestion.UsedAt = DateTime.UtcNow;

        game.Status = GameStatus.QuestionActive;
        game.CurrentTurnId = gameTurn.Id;

        if (game.StartedAt is null)
            game.StartedAt = DateTime.UtcNow;

        _dbContext.GameTurns.Add(gameTurn);

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return new SelectedQuestionResponse
        {
            GameSessionId = game.Id,
            GameTurnId = gameTurn.Id,
            GameQuestionId = gameQuestion.Id,
            CategoryId = gameQuestion.CategoryId,
            CategoryName = gameQuestion.Category.Name,
            QuestionText = gameQuestion.Question.Text,
            Difficulty = gameQuestion.Difficulty.ToString(),
            BasePoints = gameTurn.BasePoints,
            FinalPoints = gameTurn.FinalPoints,
            IsDoublePointsUsed = gameTurn.IsDoublePointsUsed,
            MainTeamTimerSeconds = game.MainTeamTimerSeconds,
            SecondTeamTimerSeconds = game.SecondTeamTimerSeconds,
            Status = gameTurn.Status.ToString(),
            ImageUrl = gameQuestion.Question.ImageUrl,
            AudioUrl = gameQuestion.Question.AudioUrl,
            VideoUrl = gameQuestion.Question.VideoUrl,
            MainTeam = new GameTurnTeamDto
            {
                Id = mainTeam.Id,
                Name = mainTeam.Name,
                Score = mainTeam.Score,
                TurnOrder = mainTeam.TurnOrder
            },
            SecondTeam = new GameTurnTeamDto
            {
                Id = secondTeam.Id,
                Name = secondTeam.Name,
                Score = secondTeam.Score,
                TurnOrder = secondTeam.TurnOrder
            }
        };
    }

    public async Task<RevealAnswerResponse> RevealAnswerAsync(
        Guid gameSessionId,
        Guid gameTurnId)
    {
        var game = await _dbContext.GameSessions
            .FirstOrDefaultAsync(x => x.Id == gameSessionId);

        if (game is null)
            throw new InvalidOperationException("Game session not found.");

        var turn = await _dbContext.GameTurns
            .Include(x => x.GameQuestion)
            .ThenInclude(x => x.Question)
            .FirstOrDefaultAsync(x =>
                x.Id == gameTurnId &&
                x.GameSessionId == gameSessionId);

        if (turn is null)
            throw new InvalidOperationException("Game turn not found.");

        if (turn.Status == TurnStatus.Completed)
            throw new InvalidOperationException("This turn is already completed.");

        turn.Status = TurnStatus.AnswerRevealed;
        game.Status = GameStatus.ReviewingAnswer;

        await _dbContext.SaveChangesAsync();

        return new RevealAnswerResponse
        {
            GameTurnId = turn.Id,
            GameQuestionId = turn.GameQuestionId,
            CorrectAnswer = turn.GameQuestion.Question.CorrectAnswer
        };
    }

    public async Task<AwardPointsResponse> AwardPointsAsync(
    Guid gameSessionId,
    Guid gameTurnId,
    AwardPointsRequest request)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var game = await _dbContext.GameSessions
            .FirstOrDefaultAsync(x => x.Id == gameSessionId);

        if (game is null)
            throw new InvalidOperationException("Game session not found.");

        var turn = await _dbContext.GameTurns
            .FirstOrDefaultAsync(x =>
                x.Id == gameTurnId &&
                x.GameSessionId == gameSessionId);

        if (turn is null)
            throw new InvalidOperationException("Game turn not found.");

        if (turn.Status == TurnStatus.Completed)
            throw new InvalidOperationException("This turn is already completed.");

        if (turn.Status != TurnStatus.AnswerRevealed)
            throw new InvalidOperationException("You must reveal the answer before awarding points.");

        var teams = await _dbContext.Teams
            .Where(x => x.GameSessionId == gameSessionId)
            .OrderBy(x => x.TurnOrder)
            .ToListAsync();

        if (teams.Count != 2)
            throw new InvalidOperationException("Game must have exactly two teams.");

        var mainTeam = teams.FirstOrDefault(x => x.Id == turn.MainTeamId);
        var secondTeam = teams.FirstOrDefault(x => x.Id == turn.SecondTeamId);

        if (mainTeam is null || secondTeam is null)
            throw new InvalidOperationException("Turn teams are invalid.");

        Team? correctTeam = null;

        if (request.CorrectTeamId.HasValue)
        {
            correctTeam = teams.FirstOrDefault(x => x.Id == request.CorrectTeamId.Value);

            if (correctTeam is null)
                throw new InvalidOperationException("Correct team does not belong to this game.");

            if (correctTeam.Id != turn.MainTeamId && correctTeam.Id != turn.SecondTeamId)
                throw new InvalidOperationException("Correct team is not part of this turn.");
        }

        var existingAttempts = await _dbContext.AnswerAttempts
            .Where(x => x.GameTurnId == gameTurnId)
            .ToListAsync();

        if (existingAttempts.Any())
        {
            _dbContext.AnswerAttempts.RemoveRange(existingAttempts);
        }

        var answerAttempts = new List<AnswerAttempt>
    {
        new AnswerAttempt
        {
            Id = Guid.NewGuid(),
            GameTurnId = turn.Id,
            TeamId = mainTeam.Id,
            AnswerText = request.MainTeamAnswerText?.Trim(),
            IsCorrect = correctTeam?.Id == mainTeam.Id,
            IsSecondChance = false,
            SubmittedAt = DateTime.UtcNow
        },
        new AnswerAttempt
        {
            Id = Guid.NewGuid(),
            GameTurnId = turn.Id,
            TeamId = secondTeam.Id,
            AnswerText = request.SecondTeamAnswerText?.Trim(),
            IsCorrect = correctTeam?.Id == secondTeam.Id,
            IsSecondChance = true,
            SubmittedAt = DateTime.UtcNow
        }
    };

        _dbContext.AnswerAttempts.AddRange(answerAttempts);

        var pointsAwarded = 0;

        if (correctTeam is not null)
        {
            pointsAwarded = turn.FinalPoints;

            correctTeam.Score += pointsAwarded;

            var scoreTransaction = new ScoreTransaction
            {
                Id = Guid.NewGuid(),
                GameSessionId = gameSessionId,
                TeamId = correctTeam.Id,
                GameTurnId = turn.Id,
                Points = pointsAwarded,
                Reason = turn.IsDoublePointsUsed
                    ? $"Correct answer with double points: +{pointsAwarded}"
                    : $"Correct answer: +{pointsAwarded}",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.ScoreTransactions.Add(scoreTransaction);
        }

        turn.Status = TurnStatus.Completed;
        turn.CompletedAt = DateTime.UtcNow;

        game.CurrentTurnId = null;
        game.Status = GameStatus.InProgress;

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return new AwardPointsResponse
        {
            GameSessionId = gameSessionId,
            GameTurnId = turn.Id,
            CorrectTeamId = correctTeam?.Id,
            PointsAwarded = pointsAwarded,
            Status = turn.Status.ToString(),
            Teams = teams
                .OrderBy(x => x.TurnOrder)
                .Select(x => new GameTurnTeamDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Score = x.Score,
                    TurnOrder = x.TurnOrder
                })
                .ToList()
        };
    }

    public async Task<UseHelpOptionResponse> UseHelpOptionAsync(
    Guid gameSessionId,
    Guid gameTurnId,
    UseHelpOptionRequest request)
    {
        if (request.TeamId == Guid.Empty)
            throw new InvalidOperationException("Team id is required.");

        if (string.IsNullOrWhiteSpace(request.Type))
            throw new InvalidOperationException("Help option type is required.");

        if (!Enum.TryParse<HelpOptionType>(request.Type, true, out var helpType))
            throw new InvalidOperationException("Invalid help option type.");

        if (helpType == HelpOptionType.DoublePoints)
            throw new InvalidOperationException("Double points must be used before selecting the question.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var turn = await _dbContext.GameTurns
            .FirstOrDefaultAsync(x =>
                x.Id == gameTurnId &&
                x.GameSessionId == gameSessionId);

        if (turn is null)
            throw new InvalidOperationException("Game turn not found.");

        if (turn.Status == TurnStatus.Completed)
            throw new InvalidOperationException("This turn is already completed.");

        if (turn.Status == TurnStatus.AnswerRevealed)
            throw new InvalidOperationException("You cannot use help option after revealing the answer.");

        if (helpType == HelpOptionType.TwoAnswers && request.TeamId != turn.MainTeamId)
            throw new InvalidOperationException("Two answers can only be used by the current turn team.");

        if (helpType == HelpOptionType.StopPlayer && request.TeamId != turn.SecondTeamId)
            throw new InvalidOperationException("Stop player can only be used by the opponent team.");

        var teamExists = await _dbContext.Teams
            .AnyAsync(x =>
                x.Id == request.TeamId &&
                x.GameSessionId == gameSessionId);

        if (!teamExists)
            throw new InvalidOperationException("Team does not belong to this game.");

        var helpOption = await _dbContext.TeamHelpOptions
            .FirstOrDefaultAsync(x =>
                x.TeamId == request.TeamId &&
                x.Type == helpType);

        if (helpOption is null)
            throw new InvalidOperationException("Help option is not available for this team.");

        if (helpOption.IsUsed)
            throw new InvalidOperationException("Help option has already been used.");

        helpOption.IsUsed = true;
        helpOption.UsedAt = DateTime.UtcNow;
        helpOption.UsedInTurnId = turn.Id;

        if (helpType == HelpOptionType.TwoAnswers)
        {
            turn.IsTwoAnswersUsed = true;
        }

        if (helpType == HelpOptionType.StopPlayer)
        {
            turn.IsStopPlayerUsed = true;

            if (request.PlayerId.HasValue)
            {
                var player = await _dbContext.Players
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.PlayerId.Value &&
                        x.TeamId == turn.MainTeamId);

                if (player is null)
                    throw new InvalidOperationException("Player not found in the current turn team.");

                player.IsStoppedForCurrentQuestion = true;
                turn.StoppedPlayerId = player.Id;
            }
        }

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return new UseHelpOptionResponse
        {
            GameTurnId = turn.Id,
            TeamId = request.TeamId,
            Type = helpType.ToString(),
            Title = GetHelpOptionTitle(helpType),
            IsUsed = true,
            UsedAt = helpOption.UsedAt!.Value
        };
    }

    private static string GetHelpOptionTitle(HelpOptionType type)
    {
        return type switch
        {
            HelpOptionType.DoublePoints => "دبل النقط",
            HelpOptionType.TwoAnswers => "إجابتين",
            HelpOptionType.StopPlayer => "إيقاف لاعب",
            _ => type.ToString()
        };
    }
}