using SeenJeemGame.Domain.Enums;

namespace SeenJeemGame.Domain.Entities;

public class GameQuestion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GameSessionId { get; set; }

    public int QuestionId { get; set; }

    public int CategoryId { get; set; }

    public QuestionDifficulty Difficulty { get; set; }

    public int Points { get; set; }

    public bool IsUsed { get; set; } = false;

    public Guid? UsedByTeamId { get; set; }

    public DateTime? UsedAt { get; set; }

    public GameSession GameSession { get; set; } = null!;

    public Question Question { get; set; } = null!;

    public Category Category { get; set; } = null!;
}