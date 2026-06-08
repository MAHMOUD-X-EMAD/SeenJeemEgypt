using System.Numerics;

namespace SeenJeemGame.Domain.Entities;

public class Team
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GameSessionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Score { get; set; } = 0;

    public int TurnOrder { get; set; }

    public GameSession GameSession { get; set; } = null!;

    public ICollection<Player> Players { get; set; } = new List<Player>();

    public ICollection<TeamHelpOption> HelpOptions { get; set; } = new List<TeamHelpOption>();

    public ICollection<ScoreTransaction> ScoreTransactions { get; set; } = new List<ScoreTransaction>();
}