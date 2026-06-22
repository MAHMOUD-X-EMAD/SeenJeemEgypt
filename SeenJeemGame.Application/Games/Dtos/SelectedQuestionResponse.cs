namespace SeenJeemGame.Application.Games.Dtos;

public class SelectedQuestionResponse
{
    public Guid GameSessionId { get; set; }

    public Guid GameTurnId { get; set; }

    public Guid GameQuestionId { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string QuestionText { get; set; } = string.Empty;

    public string Difficulty { get; set; } = string.Empty;

    public int BasePoints { get; set; }

    public int FinalPoints { get; set; }

    public bool IsDoublePointsUsed { get; set; }

    public int MainTeamTimerSeconds { get; set; }

    public int SecondTeamTimerSeconds { get; set; }

    public string Status { get; set; } = string.Empty;

    public GameTurnTeamDto MainTeam { get; set; } = new();

    public GameTurnTeamDto SecondTeam { get; set; } = new();

    public string? ImageUrl { get; set; }

    public string? AudioUrl { get; set; }

    public string? VideoUrl { get; set; }

    public string QuestionType { get; set; } = string.Empty;

    public int RevealedCluesCount { get; set; }

    public List<string> RevealedClues { get; set; } = new();

    public bool HasMoreClues { get; set; }

    public string? ClosestAnswerUnit { get; set; }

    public int RevealedRankingItemsCount { get; set; }

    public List<string> RevealedRankingItems { get; set; } = new();

    public bool HasMoreRankingItems { get; set; }

    public int? MainTeamLockedClueNumber { get; set; }

    public int? MainTeamLockedPoints { get; set; }

    public int? SecondTeamLockedClueNumber { get; set; }

    public int? SecondTeamLockedPoints { get; set; }
}