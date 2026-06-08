namespace SeenJeemGame.Domain.Entities;

public class ScoreTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GameSessionId { get; set; }

    public Guid TeamId { get; set; }

    public Guid GameTurnId { get; set; }

    public int Points { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Team Team { get; set; } = null!;

    public GameSession GameSession { get; set; } = null!;

    public GameTurn GameTurn { get; set; } = null!;
}