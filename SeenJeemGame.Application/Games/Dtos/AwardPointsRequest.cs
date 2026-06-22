namespace SeenJeemGame.Application.Games.Dtos;

public class AwardPointsRequest
{
    public Guid? CorrectTeamId { get; set; }

    public string? MainTeamAnswerText { get; set; }

    public string? SecondTeamAnswerText { get; set; }

    public int? MainTeamCorrectPositions { get; set; }

    public int? SecondTeamCorrectPositions { get; set; }

    public bool? MainTeamThreeCluesIsCorrect { get; set; }

    public bool? SecondTeamThreeCluesIsCorrect { get; set; }
}