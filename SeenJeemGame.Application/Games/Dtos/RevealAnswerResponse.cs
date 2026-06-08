namespace SeenJeemGame.Application.Games.Dtos;

public class RevealAnswerResponse
{
    public Guid GameTurnId { get; set; }

    public Guid GameQuestionId { get; set; }

    public string CorrectAnswer { get; set; } = string.Empty;
}