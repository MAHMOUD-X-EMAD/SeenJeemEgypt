using SeenJeemGame.Domain.Enums;

namespace SeenJeemGame.Domain.Entities;

public class GameSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string RoomCode { get; set; } = string.Empty;

    public GameStatus Status { get; set; } = GameStatus.Draft;

    public Guid? CurrentTurnId { get; set; }

    public int MainTeamTimerSeconds { get; set; } = 90;

    public int SecondTeamTimerSeconds { get; set; } = 15;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public ICollection<GameCategory> GameCategories { get; set; } = new List<GameCategory>();

    public ICollection<GameQuestion> GameQuestions { get; set; } = new List<GameQuestion>();

    public ICollection<Team> Teams { get; set; } = new List<Team>();

    public ICollection<GameTurn> Turns { get; set; } = new List<GameTurn>();

    public FinalRound? FinalRound { get; set; }
}