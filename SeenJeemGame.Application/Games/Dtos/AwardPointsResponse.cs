namespace SeenJeemGame.Application.Games.Dtos;

public class AwardPointsResponse
{
    public Guid GameSessionId { get; set; }

    public Guid GameTurnId { get; set; }

    public Guid? CorrectTeamId { get; set; }

    public int PointsAwarded { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<GameTurnTeamDto> Teams { get; set; } = new();

    public string QuestionType { get; set; } = string.Empty;

    public bool IsTie { get; set; }

    public List<Guid> CorrectTeamIds { get; set; } = new();

    public int? MainTeamCorrectPositions { get; set; }

    public int? SecondTeamCorrectPositions { get; set; }

    public int MainTeamPointsAwarded { get; set; }

    public int SecondTeamPointsAwarded { get; set; }
}