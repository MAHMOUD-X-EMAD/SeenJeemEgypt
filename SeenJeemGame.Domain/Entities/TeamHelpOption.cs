using SeenJeemGame.Domain.Enums;

namespace SeenJeemGame.Domain.Entities;

public class TeamHelpOption
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TeamId { get; set; }

    public HelpOptionType Type { get; set; }

    public bool IsUsed { get; set; } = false;

    public Guid? UsedInTurnId { get; set; }

    public DateTime? UsedAt { get; set; }

    public Team Team { get; set; } = null!;
}