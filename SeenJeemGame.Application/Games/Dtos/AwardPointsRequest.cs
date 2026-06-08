namespace SeenJeemGame.Application.Games.Dtos;

public class AwardPointsRequest
{
    public Guid? CorrectTeamId { get; set; }

    public string? MainTeamAnswerText { get; set; }

    public string? SecondTeamAnswerText { get; set; }
}