namespace SeenJeemGame.Application.Games.Dtos;

public class GameBoardCategoryDto
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int Order { get; set; }

    public List<GameBoardQuestionDto> Questions { get; set; } = new();
}