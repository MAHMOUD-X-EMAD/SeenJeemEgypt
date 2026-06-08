using Microsoft.EntityFrameworkCore;
using SeenJeemGame.Application.Games;
using SeenJeemGame.Application.Games.Dtos;
using SeenJeemGame.Domain.Entities;
using SeenJeemGame.Domain.Enums;
using SeenJeemGame.Infrastructure.Persistence;

namespace SeenJeemGame.Infrastructure.Services.Games;

public class GameSetupService : IGameSetupService
{
    private readonly AppDbContext _dbContext;

    public GameSetupService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GameSetupResponse> CreateGameAsync(CreateGameRequest request)
    {
        ValidateCreateGameRequest(request);

        var selectedCategoryIds = request.CategoryIds
            .Distinct()
            .ToList();

        var categories = await _dbContext.Categories
            .Where(x => selectedCategoryIds.Contains(x.Id) && x.IsActive)
            .ToListAsync();

        if (categories.Count != 6)
            throw new InvalidOperationException("You must select 6 active categories.");

        var selectedQuestions = await GetSelectedQuestionsAsync(selectedCategoryIds);

        var gameSession = new GameSession
        {
            Id = Guid.NewGuid(),
            RoomCode = await GenerateUniqueRoomCodeAsync(),
            Status = GameStatus.WaitingToStart,
            MainTeamTimerSeconds = 90,
            SecondTeamTimerSeconds = 15,
            CreatedAt = DateTime.UtcNow
        };

        var teamOne = new Team
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameSession.Id,
            Name = request.TeamOneName.Trim(),
            Score = 0,
            TurnOrder = 1
        };

        var teamTwo = new Team
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameSession.Id,
            Name = request.TeamTwoName.Trim(),
            Score = 0,
            TurnOrder = 2
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        _dbContext.GameSessions.Add(gameSession);

        var gameCategories = selectedCategoryIds
            .Select((categoryId, index) => new GameCategory
            {
                Id = Guid.NewGuid(),
                GameSessionId = gameSession.Id,
                CategoryId = categoryId,
                Order = index + 1
            })
            .ToList();

        _dbContext.GameCategories.AddRange(gameCategories);

        _dbContext.Teams.AddRange(teamOne, teamTwo);

        var helpOptions = request.HelpOptions
            .Distinct()
            .SelectMany(helpType => new[]
            {
                new TeamHelpOption
                {
                    Id = Guid.NewGuid(),
                    TeamId = teamOne.Id,
                    Type = helpType,
                    IsUsed = false
                },
                new TeamHelpOption
                {
                    Id = Guid.NewGuid(),
                    TeamId = teamTwo.Id,
                    Type = helpType,
                    IsUsed = false
                }
            })
            .ToList();

        _dbContext.TeamHelpOptions.AddRange(helpOptions);

        var gameQuestions = selectedQuestions
            .Select(question => new GameQuestion
            {
                Id = Guid.NewGuid(),
                GameSessionId = gameSession.Id,
                QuestionId = question.Id,
                CategoryId = question.CategoryId,
                Difficulty = question.Difficulty,
                Points = question.Points,
                IsUsed = false
            })
            .ToList();

        _dbContext.GameQuestions.AddRange(gameQuestions);

        await _dbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        var response = await GetGameBoardAsync(gameSession.Id);

        return response!;
    }

    public async Task<GameSetupResponse?> GetGameBoardAsync(Guid gameSessionId)
    {
        var game = await _dbContext.GameSessions
            .AsNoTracking()
            .Where(x => x.Id == gameSessionId)
            .Select(x => new
            {
                x.Id,
                x.RoomCode,
                x.CurrentTurnId,
                x.Status
            })
            .FirstOrDefaultAsync();

        if (game is null)
            return null;

        var teams = await _dbContext.Teams
    .Include(x => x.HelpOptions)
    .Where(x => x.GameSessionId == gameSessionId)
    .OrderBy(x => x.TurnOrder)
    .ToListAsync();

        var completedTurnsCount = await _dbContext.GameTurns
            .CountAsync(x =>
                x.GameSessionId == gameSessionId &&
                x.Status == TurnStatus.Completed);

        Team? currentTurnTeam = null;

        if (game.CurrentTurnId.HasValue)
        {
            var activeTurn = await _dbContext.GameTurns
                .FirstOrDefaultAsync(x =>
                    x.Id == game.CurrentTurnId.Value &&
                    x.GameSessionId == gameSessionId &&
                    x.Status != TurnStatus.Completed);

            if (activeTurn is not null)
            {
                currentTurnTeam = teams.FirstOrDefault(x => x.Id == activeTurn.MainTeamId);
            }
        }

        if (currentTurnTeam is null && teams.Count == 2)
        {
            currentTurnTeam = completedTurnsCount % 2 == 0
                ? teams.First(x => x.TurnOrder == 1)
                : teams.First(x => x.TurnOrder == 2);
        }

        var categories = await _dbContext.GameCategories
            .AsNoTracking()
            .Where(x => x.GameSessionId == gameSessionId)
            .OrderBy(x => x.Order)
            .Select(x => new GameBoardCategoryDto
            {
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                Order = x.Order,
                Questions = _dbContext.GameQuestions
                    .Where(q => q.GameSessionId == gameSessionId && q.CategoryId == x.CategoryId)
                    .OrderBy(q => q.Difficulty)
                    .ThenBy(q => q.Points)
                    .Select(q => new GameBoardQuestionDto
                    {
                        GameQuestionId = q.Id,
                        Difficulty = q.Difficulty.ToString(),
                        Points = q.Points,
                        IsUsed = q.IsUsed
                    })
                    .ToList()
            })
            .ToListAsync();

        return new GameSetupResponse
        {
            GameSessionId = game.Id,
            RoomCode = game.RoomCode,
            Status = game.Status.ToString(),

            CurrentTurnTeamId = currentTurnTeam?.Id,
            CurrentTurnTeamName = currentTurnTeam?.Name,
            CurrentTurnOrder = currentTurnTeam?.TurnOrder,

            Teams = teams.Select(team => new GameTeamDto
            {
                Id = team.Id,
                Name = team.Name,
                Score = team.Score,
                TurnOrder = team.TurnOrder,
                HelpOptions = team.HelpOptions
                    .OrderBy(x => x.Type)
                    .Select(x => new TeamHelpOptionDto
                    {
                        Id = x.Id,
                        Type = x.Type.ToString(),
                        Title = GetHelpOptionTitle(x.Type),
                        IsUsed = x.IsUsed,
                        UsedAt = x.UsedAt
                    })
                    .ToList()
            }).ToList(),

            Categories = categories
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

    private async Task<List<Question>> GetSelectedQuestionsAsync(List<int> categoryIds)
    {
        var allQuestions = await _dbContext.Questions
            .Where(x => categoryIds.Contains(x.CategoryId) && x.IsActive)
            .ToListAsync();

        var selectedQuestions = new List<Question>();

        foreach (var categoryId in categoryIds)
        {
            foreach (var difficulty in new[]
                     {
                         QuestionDifficulty.Easy,
                         QuestionDifficulty.Medium,
                         QuestionDifficulty.Hard
                     })
            {
                var questions = allQuestions
                    .Where(x => x.CategoryId == categoryId && x.Difficulty == difficulty)
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(2)
                    .ToList();

                if (questions.Count < 2)
                {
                    throw new InvalidOperationException(
                        $"Category {categoryId} must have at least 2 active questions for difficulty {difficulty}.");
                }

                selectedQuestions.AddRange(questions);
            }
        }

        return selectedQuestions;
    }

    private static void ValidateCreateGameRequest(CreateGameRequest request)
    {
        if (request.CategoryIds is null || request.CategoryIds.Distinct().Count() != 6)
            throw new InvalidOperationException("You must select exactly 6 different categories.");

        if (string.IsNullOrWhiteSpace(request.TeamOneName))
            throw new InvalidOperationException("Team one name is required.");

        if (string.IsNullOrWhiteSpace(request.TeamTwoName))
            throw new InvalidOperationException("Team two name is required.");

        if (request.TeamOneName.Trim().Equals(request.TeamTwoName.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Team names must be different.");

        if (request.HelpOptions is null)
            request.HelpOptions = new List<HelpOptionType>();

        foreach (var helpOption in request.HelpOptions)
        {
            if (!Enum.IsDefined(typeof(HelpOptionType), helpOption))
                throw new InvalidOperationException("Invalid help option.");
        }
    }

    private async Task<string> GenerateUniqueRoomCodeAsync()
    {
        string roomCode;

        do
        {
            roomCode = GenerateRoomCode();
        }
        while (await _dbContext.GameSessions.AnyAsync(x => x.RoomCode == roomCode));

        return roomCode;
    }

    private static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        var random = new Random();

        return new string(Enumerable
            .Range(0, 6)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
    }
}