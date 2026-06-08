namespace SeenJeemGame.Application.Games.Dtos;

public class AdjustTeamScoreRequest
{
    public Guid TeamId { get; set; }

    public int PointsDelta { get; set; }
}