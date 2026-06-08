namespace SeenJeemGame.Application.Games.Dtos;

public class TeamHelpOptionDto
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public bool IsUsed { get; set; }

    public DateTime? UsedAt { get; set; }
}