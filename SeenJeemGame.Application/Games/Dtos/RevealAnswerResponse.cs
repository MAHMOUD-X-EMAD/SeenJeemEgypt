namespace SeenJeemGame.Application.Games.Dtos;

public class RevealAnswerResponse
{
    public Guid GameTurnId { get; set; }

    public Guid GameQuestionId { get; set; }

    public string CorrectAnswer { get; set; } = string.Empty;

    public string QuestionType { get; set; } = string.Empty;

    public decimal? NumericAnswer { get; set; }

    public string? Unit { get; set; }
}