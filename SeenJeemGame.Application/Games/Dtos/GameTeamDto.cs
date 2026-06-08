namespace SeenJeemGame.Application.Games.Dtos;

public class GameTeamDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Score { get; set; }

    public int TurnOrder { get; set; }

    public List<string> HelpOptions { get; set; } = new();
}