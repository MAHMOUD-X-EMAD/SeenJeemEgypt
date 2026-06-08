namespace SeenJeemGame.Domain.Entities;

public class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TeamId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsStoppedForCurrentQuestion { get; set; } = false;

    public Team Team { get; set; } = null!;
}