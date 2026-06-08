namespace SeenJeemGame.Application.Games.Dtos;

public class UseHelpOptionResponse
{
    public Guid GameTurnId { get; set; }

    public Guid TeamId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public bool IsUsed { get; set; }

    public DateTime UsedAt { get; set; }
}