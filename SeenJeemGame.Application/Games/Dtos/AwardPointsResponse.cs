namespace SeenJeemGame.Application.Games.Dtos;

public class AwardPointsResponse
{
    public Guid GameSessionId { get; set; }

    public Guid GameTurnId { get; set; }

    public Guid? CorrectTeamId { get; set; }

    public int PointsAwarded { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<GameTurnTeamDto> Teams { get; set; } = new();
}