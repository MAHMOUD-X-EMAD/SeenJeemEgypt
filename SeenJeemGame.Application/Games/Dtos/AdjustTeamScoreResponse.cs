namespace SeenJeemGame.Application.Games.Dtos;

public class AdjustTeamScoreResponse
{
    public Guid GameSessionId { get; set; }

    public Guid TeamId { get; set; }

    public int PointsDelta { get; set; }

    public int NewScore { get; set; }

    public List<GameTurnTeamDto> Teams { get; set; } = new();
}