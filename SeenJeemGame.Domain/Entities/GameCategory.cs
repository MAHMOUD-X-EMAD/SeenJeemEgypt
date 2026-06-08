namespace SeenJeemGame.Domain.Entities;

public class GameCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GameSessionId { get; set; }

    public int CategoryId { get; set; }

    public int Order { get; set; }

    public GameSession GameSession { get; set; } = null!;

    public Category Category { get; set; } = null!;
}