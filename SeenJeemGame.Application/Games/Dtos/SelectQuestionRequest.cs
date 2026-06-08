namespace SeenJeemGame.Application.Games.Dtos;

public class SelectQuestionRequest
{
    public Guid GameQuestionId { get; set; }

    public bool UseDoublePoints { get; set; } = false;
}