using Microsoft.EntityFrameworkCore;
using SeenJeemGame.Application.Games;
using SeenJeemGame.Application.Games.Dtos;
using SeenJeemGame.Application.Games.QuestionMetadata;
using SeenJeemGame.Domain.Entities;
using SeenJeemGame.Domain.Enums;
using SeenJeemGame.Infrastructure.Persistence;
using System.Globalization;
using System.Text.Json;

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

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        var game = await _dbContext.GameSessions
            .FirstOrDefaultAsync(x => x.Id == gameSessionId);

        if (game is null)
            throw new InvalidOperationException("Game session not found.");

        if (game.Status == GameStatus.Finished ||
            game.Status == GameStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Game is already finished or cancelled.");
        }

        if (game.CurrentTurnId.HasValue)
        {
            var currentTurnIsStillActive = await _dbContext.GameTurns
                .AnyAsync(x =>
                    x.Id == game.CurrentTurnId.Value &&
                    x.Status != TurnStatus.Completed);

            // لو عايز تمنع فتح سؤال جديد أثناء وجود سؤال نشط،
            // رجع التحقق ده.
            // if (currentTurnIsStillActive)
            //     throw new InvalidOperationException(
            //         "There is already an active question.");
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
            throw new InvalidOperationException(
                "This question has already been used.");

        var teams = await _dbContext.Teams
            .Where(x => x.GameSessionId == gameSessionId)
            .OrderBy(x => x.TurnOrder)
            .ToListAsync();

        if (teams.Count != 2)
            throw new InvalidOperationException(
                "Game must have exactly two teams.");

        var completedTurnsCount = await _dbContext.GameTurns
            .CountAsync(x =>
                x.GameSessionId == gameSessionId &&
                x.Status == TurnStatus.Completed);

        var mainTeam = completedTurnsCount % 2 == 0
            ? teams.First(x => x.TurnOrder == 1)
            : teams.First(x => x.TurnOrder == 2);

        var secondTeam = teams.First(x => x.Id != mainTeam.Id);

        var gameTurnId = Guid.NewGuid();
        var isDoublePointsUsed = false;

        if (request.UseDoublePoints)
        {
            var doublePointsOption = await _dbContext.TeamHelpOptions
                .FirstOrDefaultAsync(x =>
                    x.TeamId == mainTeam.Id &&
                    x.Type == HelpOptionType.DoublePoints);

            if (doublePointsOption is null)
            {
                throw new InvalidOperationException(
                    "Double points help option is not available for this team.");
            }

            if (doublePointsOption.IsUsed)
            {
                throw new InvalidOperationException(
                    "Double points help option has already been used.");
            }

            doublePointsOption.IsUsed = true;
            doublePointsOption.UsedAt = DateTime.UtcNow;
            doublePointsOption.UsedInTurnId = gameTurnId;

            isDoublePointsUsed = true;
        }

        var questionType = gameQuestion.Question.QuestionType;
        var basePoints = gameQuestion.Points;

        var pointsBeforeDouble = basePoints;

        var revealedCluesCount = 0;
        var revealedClues = new List<string>();
        var hasMoreClues = false;

        int? clueAdjustedPoints = null;
        string? closestAnswerUnit = null;

        if (questionType == QuestionType.ThreeClues)
        {
            var metadata = GetThreeCluesMetadata(gameQuestion.Question);

            // أول تلميح يظهر مباشرة عند فتح السؤال.
            revealedCluesCount = 1;
            revealedClues.Add(metadata.Clues[0]);

            clueAdjustedPoints = metadata.PointsByClue[0];
            pointsBeforeDouble = clueAdjustedPoints.Value;

            hasMoreClues = metadata.Clues.Count > revealedCluesCount;
        }
        else if (questionType == QuestionType.ClosestAnswer)
        {
            var metadata = GetClosestAnswerMetadata(gameQuestion.Question);

            // نرسل الوحدة فقط، ولا نرسل الرقم الصحيح.
            closestAnswerUnit = metadata.Unit;
        }

        var finalPoints = isDoublePointsUsed
            ? pointsBeforeDouble * 2
            : pointsBeforeDouble;

        var gameTurn = new GameTurn
        {
            Id = gameTurnId,
            GameSessionId = gameSessionId,
            GameQuestionId = gameQuestion.Id,
            MainTeamId = mainTeam.Id,
            SecondTeamId = secondTeam.Id,

            Status = TurnStatus.MainTeamAnswering,

            IsDoublePointsUsed = isDoublePointsUsed,

            BasePoints = basePoints,
            FinalPoints = finalPoints,

            RevealedCluesCount = revealedCluesCount,
            ClueAdjustedPoints = clueAdjustedPoints,

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
            QuestionType = questionType.ToString(),

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

            RevealedCluesCount = gameTurn.RevealedCluesCount,
            RevealedClues = revealedClues,
            HasMoreClues = hasMoreClues,

            ClosestAnswerUnit = closestAnswerUnit,

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

    public async Task<RevealNextClueResponse> RevealNextClueAsync(
    Guid gameSessionId,
    Guid gameTurnId)
    {
        var turn = await _dbContext.GameTurns
            .Include(x => x.GameQuestion)
            .ThenInclude(x => x.Question)
            .FirstOrDefaultAsync(x =>
                x.Id == gameTurnId &&
                x.GameSessionId == gameSessionId);

        if (turn is null)
            throw new InvalidOperationException("Game turn not found.");

        if (turn.Status == TurnStatus.Completed)
            throw new InvalidOperationException(
                "This turn is already completed.");

        if (turn.Status == TurnStatus.AnswerRevealed)
        {
            throw new InvalidOperationException(
                "You cannot reveal another clue after revealing the answer.");
        }

        var question = turn.GameQuestion.Question;

        if (question.QuestionType != QuestionType.ThreeClues)
        {
            throw new InvalidOperationException(
                "This question does not support clues.");
        }

        var metadata = GetThreeCluesMetadata(question);

        if (turn.RevealedCluesCount <= 0)
            turn.RevealedCluesCount = 1;

        if (turn.RevealedCluesCount >= metadata.Clues.Count)
        {
            throw new InvalidOperationException(
                "All clues have already been revealed.");
        }

        // لو RevealedCluesCount = 1، يبقى الـindex القادم هو 1.
        var nextClueIndex = turn.RevealedCluesCount;

        turn.RevealedCluesCount++;

        var adjustedPoints = metadata.PointsByClue[nextClueIndex];

        turn.ClueAdjustedPoints = adjustedPoints;

        turn.FinalPoints = turn.IsDoublePointsUsed
            ? adjustedPoints * 2
            : adjustedPoints;

        await _dbContext.SaveChangesAsync();

        return new RevealNextClueResponse
        {
            GameTurnId = turn.Id,

            RevealedCluesCount = turn.RevealedCluesCount,

            RevealedClues = metadata.Clues
                .Take(turn.RevealedCluesCount)
                .ToList(),

            ClueAdjustedPoints = adjustedPoints,

            FinalPoints = turn.FinalPoints,

            HasMoreClues =
                turn.RevealedCluesCount < metadata.Clues.Count
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
            throw new InvalidOperationException(
                "This turn is already completed.");

        var question = turn.GameQuestion.Question;

        decimal? numericAnswer = null;
        string? unit = null;

        if (question.QuestionType == QuestionType.ClosestAnswer)
        {
            var metadata = GetClosestAnswerMetadata(question);

            numericAnswer = metadata.NumericAnswer;
            unit = metadata.Unit;
        }

        turn.Status = TurnStatus.AnswerRevealed;
        game.Status = GameStatus.ReviewingAnswer;

        await _dbContext.SaveChangesAsync();

        return new RevealAnswerResponse
        {
            GameTurnId = turn.Id,
            GameQuestionId = turn.GameQuestionId,

            CorrectAnswer = question.CorrectAnswer,
            QuestionType = question.QuestionType.ToString(),

            NumericAnswer = numericAnswer,
            Unit = unit
        };
    }
    public async Task<AwardPointsResponse> AwardPointsAsync(
    Guid gameSessionId,
    Guid gameTurnId,
    AwardPointsRequest request)
    {
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

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
            throw new InvalidOperationException(
                "This turn is already completed.");

        if (turn.Status != TurnStatus.AnswerRevealed)
        {
            throw new InvalidOperationException(
                "You must reveal the answer before awarding points.");
        }

        var teams = await _dbContext.Teams
            .Where(x => x.GameSessionId == gameSessionId)
            .OrderBy(x => x.TurnOrder)
            .ToListAsync();

        if (teams.Count != 2)
        {
            throw new InvalidOperationException(
                "Game must have exactly two teams.");
        }

        var mainTeam = teams.FirstOrDefault(
            x => x.Id == turn.MainTeamId);

        var secondTeam = teams.FirstOrDefault(
            x => x.Id == turn.SecondTeamId);

        if (mainTeam is null || secondTeam is null)
            throw new InvalidOperationException("Turn teams are invalid.");

        var question = turn.GameQuestion.Question;

        var isClosestAnswer =
            question.QuestionType == QuestionType.ClosestAnswer;

        if (isClosestAnswer && turn.IsTrapUsed)
        {
            throw new InvalidOperationException(
                "Trap cannot be used with closest-answer questions.");
        }

        Team? correctTeam = null;

        var correctTeams = new List<Team>();

        var isTie = false;

        decimal? mainNumericAnswer = null;
        decimal? secondNumericAnswer = null;
        decimal? correctNumericAnswer = null;

        /*
         * ClosestAnswer:
         * الفائز بيتحدد تلقائيًا من إجابتي الفريقين.
         */
        if (isClosestAnswer)
        {
            var metadata = GetClosestAnswerMetadata(question);

            correctNumericAnswer = metadata.NumericAnswer;

            mainNumericAnswer = ParseNumericAnswer(
                request.MainTeamAnswerText,
                mainTeam.Name);

            secondNumericAnswer = ParseNumericAnswer(
                request.SecondTeamAnswerText,
                secondTeam.Name);

            var mainDifference = Math.Abs(
                mainNumericAnswer.Value -
                correctNumericAnswer.Value);

            var secondDifference = Math.Abs(
                secondNumericAnswer.Value -
                correctNumericAnswer.Value);

            if (mainDifference < secondDifference)
            {
                correctTeam = mainTeam;
                correctTeams.Add(mainTeam);
            }
            else if (secondDifference < mainDifference)
            {
                correctTeam = secondTeam;
                correctTeams.Add(secondTeam);
            }
            else
            {
                isTie = true;

                correctTeams.Add(mainTeam);
                correctTeams.Add(secondTeam);
            }
        }
        else
        {
            /*
             * Standard وThreeClues:
             * الـHost هو اللي بيحدد الفريق الصحيح.
             */
            if (request.CorrectTeamId.HasValue)
            {
                correctTeam = teams.FirstOrDefault(
                    x => x.Id == request.CorrectTeamId.Value);

                if (correctTeam is null)
                {
                    throw new InvalidOperationException(
                        "Correct team does not belong to this game.");
                }

                if (correctTeam.Id != turn.MainTeamId &&
                    correctTeam.Id != turn.SecondTeamId)
                {
                    throw new InvalidOperationException(
                        "Correct team is not part of this turn.");
                }

                correctTeams.Add(correctTeam);
            }

            if (turn.IsTrapUsed)
            {
                var trapTargetTeamId =
                    turn.TrapTargetTeamId ?? turn.SecondTeamId;

                if (correctTeam is not null &&
                    correctTeam.Id != trapTargetTeamId)
                {
                    throw new InvalidOperationException(
                        "After using trap, only the opponent team can answer.");
                }
            }
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
        new()
        {
            Id = Guid.NewGuid(),
            GameTurnId = turn.Id,
            TeamId = mainTeam.Id,
            AnswerText = request.MainTeamAnswerText?.Trim(),
            IsCorrect = correctTeams.Any(
                x => x.Id == mainTeam.Id),
            IsSecondChance = false,
            SubmittedAt = DateTime.UtcNow
        },
        new()
        {
            Id = Guid.NewGuid(),
            GameTurnId = turn.Id,
            TeamId = secondTeam.Id,
            AnswerText = request.SecondTeamAnswerText?.Trim(),
            IsCorrect = correctTeams.Any(
                x => x.Id == secondTeam.Id),
            IsSecondChance = !isClosestAnswer,
            SubmittedAt = DateTime.UtcNow
        }
    };

        _dbContext.AnswerAttempts.AddRange(answerAttempts);

        var pointsAwarded = 0;

        /*
         * حساب أقرب إجابة.
         */
        if (isClosestAnswer)
        {
            pointsAwarded = turn.FinalPoints;

            foreach (var winningTeam in correctTeams)
            {
                winningTeam.Score += turn.FinalPoints;

                var reason = isTie
                    ? $"Closest answer tie: +{turn.FinalPoints}"
                    : $"Closest answer: +{turn.FinalPoints}";

                _dbContext.ScoreTransactions.Add(
                    new ScoreTransaction
                    {
                        Id = Guid.NewGuid(),
                        GameSessionId = gameSessionId,
                        TeamId = winningTeam.Id,
                        GameTurnId = turn.Id,
                        Points = turn.FinalPoints,
                        Reason = reason,
                        CreatedAt = DateTime.UtcNow
                    });
            }
        }

        /*
         * حساب الفخ.
         */
        else if (turn.IsTrapUsed)
        {
            var trapTargetTeamId =
                turn.TrapTargetTeamId ?? turn.SecondTeamId;

            var trapTargetTeam = teams.FirstOrDefault(
                x => x.Id == trapTargetTeamId);

            if (trapTargetTeam is null)
            {
                throw new InvalidOperationException(
                    "Trap target team not found.");
            }

            if (correctTeam is not null &&
                correctTeam.Id == trapTargetTeam.Id)
            {
                pointsAwarded = turn.FinalPoints;

                trapTargetTeam.Score += pointsAwarded;

                _dbContext.ScoreTransactions.Add(
                    new ScoreTransaction
                    {
                        Id = Guid.NewGuid(),
                        GameSessionId = gameSessionId,
                        TeamId = trapTargetTeam.Id,
                        GameTurnId = turn.Id,
                        Points = pointsAwarded,
                        Reason =
                            $"Trap answered correctly: +{pointsAwarded}",
                        CreatedAt = DateTime.UtcNow
                    });
            }
            else
            {
                pointsAwarded = -turn.FinalPoints;

                trapTargetTeam.Score += pointsAwarded;

                _dbContext.ScoreTransactions.Add(
                    new ScoreTransaction
                    {
                        Id = Guid.NewGuid(),
                        GameSessionId = gameSessionId,
                        TeamId = trapTargetTeam.Id,
                        GameTurnId = turn.Id,
                        Points = pointsAwarded,
                        Reason = $"Trap failed answer: {pointsAwarded}",
                        CreatedAt = DateTime.UtcNow
                    });
            }
        }

        /*
         * السؤال العادي وThreeClues.
         *
         * ThreeClues بيستخدم turn.FinalPoints،
         * وهي أصلًا اتعدلت حسب عدد التلميحات.
         */
        else if (correctTeam is not null)
        {
            pointsAwarded = turn.FinalPoints;

            correctTeam.Score += pointsAwarded;

            string reason;

            if (question.QuestionType == QuestionType.ThreeClues)
            {
                reason = turn.IsDoublePointsUsed
                    ? $"Three clues correct answer with double points: +{pointsAwarded}"
                    : $"Three clues correct answer after {turn.RevealedCluesCount} clue(s): +{pointsAwarded}";
            }
            else
            {
                reason = turn.IsDoublePointsUsed
                    ? $"Correct answer with double points: +{pointsAwarded}"
                    : $"Correct answer: +{pointsAwarded}";
            }

            _dbContext.ScoreTransactions.Add(
                new ScoreTransaction
                {
                    Id = Guid.NewGuid(),
                    GameSessionId = gameSessionId,
                    TeamId = correctTeam.Id,
                    GameTurnId = turn.Id,
                    Points = pointsAwarded,
                    Reason = reason,
                    CreatedAt = DateTime.UtcNow
                });
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

            QuestionType = question.QuestionType.ToString(),

            CorrectTeamId = correctTeams.Count == 1
                ? correctTeams[0].Id
                : null,

            CorrectTeamIds = correctTeams
                .Select(x => x.Id)
                .ToList(),

            IsTie = isTie,

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
            .Include(x => x.GameQuestion)
            .ThenInclude(x => x.Question)
            .FirstOrDefaultAsync(x =>
                x.Id == gameTurnId &&
                x.GameSessionId == gameSessionId);

        if (turn is null)
            throw new InvalidOperationException("Game turn not found.");

        if (turn.GameQuestion.Question.QuestionType ==
                QuestionType.ClosestAnswer &&
            helpType is HelpOptionType.TwoAnswers or
                        HelpOptionType.StopPlayer or
                        HelpOptionType.Trap)
                {
                    throw new InvalidOperationException(
                        "This help option cannot be used with closest-answer questions.");
                }

        if (turn.Status == TurnStatus.Completed)
            throw new InvalidOperationException("This turn is already completed.");

        if (turn.Status == TurnStatus.AnswerRevealed)
            throw new InvalidOperationException("You cannot use help option after revealing the answer.");

        if (helpType == HelpOptionType.TwoAnswers && request.TeamId != turn.MainTeamId)
            throw new InvalidOperationException("Two answers can only be used by the current turn team.");

        if (helpType == HelpOptionType.StopPlayer && request.TeamId != turn.SecondTeamId)
            throw new InvalidOperationException("Stop player can only be used by the opponent team.");

        if (helpType == HelpOptionType.Trap && request.TeamId != turn.MainTeamId)
            throw new InvalidOperationException("Trap can only be used by the current turn team.");

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

        if (helpType == HelpOptionType.Trap)
        {
            if (turn.IsTrapUsed)
                throw new InvalidOperationException("Trap has already been used in this turn.");

            turn.IsTrapUsed = true;
            turn.TrapTargetTeamId = turn.SecondTeamId;
        }

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
            HelpOptionType.Trap => "فخ",
            _ => type.ToString()
        };
    }

    public async Task<AdjustTeamScoreResponse> AdjustTeamScoreAsync(
    Guid gameSessionId,
    AdjustTeamScoreRequest request)
    {
        if (request.TeamId == Guid.Empty)
            throw new InvalidOperationException("Team id is required.");

        if (request.PointsDelta != 100 && request.PointsDelta != -100)
            throw new InvalidOperationException("Only +100 or -100 is allowed.");

        var gameExists = await _dbContext.GameSessions
            .AnyAsync(x => x.Id == gameSessionId);

        if (!gameExists)
            throw new InvalidOperationException("Game session not found.");

        var team = await _dbContext.Teams
            .FirstOrDefaultAsync(x =>
                x.Id == request.TeamId &&
                x.GameSessionId == gameSessionId);

        if (team is null)
            throw new InvalidOperationException("Team not found in this game.");

        team.Score += request.PointsDelta;

        if (team.Score < 0)
        {
            team.Score = 0;
        }

        await _dbContext.SaveChangesAsync();

        var teams = await _dbContext.Teams
            .Where(x => x.GameSessionId == gameSessionId)
            .OrderBy(x => x.TurnOrder)
            .Select(x => new GameTurnTeamDto
            {
                Id = x.Id,
                Name = x.Name,
                Score = x.Score,
                TurnOrder = x.TurnOrder
            })
            .ToListAsync();

        return new AdjustTeamScoreResponse
        {
            GameSessionId = gameSessionId,
            TeamId = team.Id,
            PointsDelta = request.PointsDelta,
            NewScore = team.Score,
            Teams = teams
        };
    }

    private static readonly JsonSerializerOptions MetadataJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static ThreeCluesMetadata GetThreeCluesMetadata(
        Question question)
    {
        if (string.IsNullOrWhiteSpace(question.MetadataJson))
        {
            throw new InvalidOperationException(
                $"Three-clues metadata is missing for question {question.Id}.");
        }

        ThreeCluesMetadata? metadata;

        try
        {
            metadata = JsonSerializer.Deserialize<ThreeCluesMetadata>(
                question.MetadataJson,
                MetadataJsonOptions);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"Three-clues metadata is invalid for question {question.Id}.");
        }

        if (metadata is null ||
            metadata.Clues.Count == 0 ||
            metadata.PointsByClue.Count == 0)
        {
            throw new InvalidOperationException(
                $"Three-clues metadata is incomplete for question {question.Id}.");
        }

        if (metadata.Clues.Count != metadata.PointsByClue.Count)
        {
            throw new InvalidOperationException(
                $"Clues and points count must match for question {question.Id}.");
        }

        if (metadata.Clues.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException(
                $"Question {question.Id} contains an empty clue.");
        }

        if (metadata.PointsByClue.Any(x => x <= 0))
        {
            throw new InvalidOperationException(
                $"Question {question.Id} contains invalid clue points.");
        }

        return metadata;
    }

    private static ClosestAnswerMetadata GetClosestAnswerMetadata(
        Question question)
    {
        if (string.IsNullOrWhiteSpace(question.MetadataJson))
        {
            throw new InvalidOperationException(
                $"Closest-answer metadata is missing for question {question.Id}.");
        }

        ClosestAnswerMetadata? metadata;

        try
        {
            metadata = JsonSerializer.Deserialize<ClosestAnswerMetadata>(
                question.MetadataJson,
                MetadataJsonOptions);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"Closest-answer metadata is invalid for question {question.Id}.");
        }

        if (metadata is null)
        {
            throw new InvalidOperationException(
                $"Closest-answer metadata is incomplete for question {question.Id}.");
        }

        return metadata;
    }

    private static decimal ParseNumericAnswer(
    string? answerText,
    string teamName)
    {
        if (string.IsNullOrWhiteSpace(answerText))
        {
            throw new InvalidOperationException(
                $"Numeric answer is required for team {teamName}.");
        }

        var normalized = ConvertArabicDigitsToEnglish(
            answerText.Trim());

        normalized = normalized
            .Replace(" ", string.Empty)
            .Replace("٬", string.Empty)
            .Replace("٫", ".");

        /*
         * يدعم:
         * 828
         * 828.5
         * ٨٢٨
         * ٨٢٨٫٥
         * 8,849
         */
        if (normalized.Contains(',') &&
            !normalized.Contains('.'))
        {
            var commaIndex = normalized.LastIndexOf(',');

            var digitsAfterComma =
                normalized.Length - commaIndex - 1;

            normalized = digitsAfterComma is 1 or 2
                ? normalized.Replace(',', '.')
                : normalized.Replace(",", string.Empty);
        }
        else
        {
            normalized = normalized.Replace(",", string.Empty);
        }

        if (!decimal.TryParse(
                normalized,
                NumberStyles.AllowLeadingSign |
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var numericAnswer))
        {
            throw new InvalidOperationException(
                $"Invalid numeric answer for team {teamName}.");
        }

        return numericAnswer;
    }

    private static string ConvertArabicDigitsToEnglish(
        string value)
    {
        return value
            .Replace('٠', '0')
            .Replace('١', '1')
            .Replace('٢', '2')
            .Replace('٣', '3')
            .Replace('٤', '4')
            .Replace('٥', '5')
            .Replace('٦', '6')
            .Replace('٧', '7')
            .Replace('٨', '8')
            .Replace('٩', '9')
            .Replace('۰', '0')
            .Replace('۱', '1')
            .Replace('۲', '2')
            .Replace('۳', '3')
            .Replace('۴', '4')
            .Replace('۵', '5')
            .Replace('۶', '6')
            .Replace('۷', '7')
            .Replace('۸', '8')
            .Replace('۹', '9');
    }
}