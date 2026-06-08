namespace SeenJeemGame.Application.Games.Dtos;

public class GameBoardQuestionDto
{
    public Guid GameQuestionId { get; set; }

    public string Difficulty { get; set; } = string.Empty;

    public int Points { get; set; }

    public bool IsUsed { get; set; }
}