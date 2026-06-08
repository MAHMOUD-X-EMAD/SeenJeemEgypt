namespace SeenJeemGame.Domain.Entities;

public class AnswerAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GameTurnId { get; set; }

    public Guid TeamId { get; set; }

    public string? AnswerText { get; set; }

    public bool? IsCorrect { get; set; }

    public bool IsSecondChance { get; set; } = false;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public GameTurn GameTurn { get; set; } = null!;

    public Team Team { get; set; } = null!;
}