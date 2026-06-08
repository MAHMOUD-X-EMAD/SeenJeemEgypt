namespace SeenJeemGame.Application.Games.Dtos;

public class UseHelpOptionRequest
{
    public Guid TeamId { get; set; }

    public string Type { get; set; } = string.Empty;

    public Guid? PlayerId { get; set; }
}