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
}