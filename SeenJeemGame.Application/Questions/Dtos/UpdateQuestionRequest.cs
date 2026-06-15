using SeenJeemGame.Domain.Enums;

namespace SeenJeemGame.Application.Questions.Dtos;

public class UpdateQuestionRequest
{
    public int CategoryId { get; set; }

    public string Text { get; set; } = string.Empty;

    public string CorrectAnswer { get; set; } = string.Empty;

    public QuestionDifficulty Difficulty { get; set; }

    public string? ImageUrl { get; set; }

    public string? AudioUrl { get; set; }

    public string? VideoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public QuestionType QuestionType { get; set; }
    = QuestionType.Standard;

    public string? MetadataJson { get; set; }
}