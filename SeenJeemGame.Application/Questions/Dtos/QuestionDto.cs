using SeenJeemGame.Domain.Enums;

namespace SeenJeemGame.Application.Questions.Dtos;

public class QuestionDto
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public string CorrectAnswer { get; set; } = string.Empty;

    public QuestionDifficulty Difficulty { get; set; }

    public string DifficultyName => Difficulty.ToString();

    public int Points { get; set; }

    public string? ImageUrl { get; set; }

    public string? AudioUrl { get; set; }

    public string? VideoUrl { get; set; }

    public bool IsActive { get; set; }
}