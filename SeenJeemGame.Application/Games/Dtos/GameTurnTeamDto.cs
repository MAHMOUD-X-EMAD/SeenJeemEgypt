namespace SeenJeemGame.Application.Games.Dtos;

public class GameTurnTeamDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Score { get; set; }

    public int TurnOrder { get; set; }
}