using SeenJeemGame.Domain.Enums;

namespace SeenJeemGame.Domain.Entities;

public class Question
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Text { get; set; } = string.Empty;

    public string CorrectAnswer { get; set; } = string.Empty;

    public QuestionDifficulty Difficulty { get; set; }

    public int Points { get; set; }

    public string? ImageUrl { get; set; }

    public string? AudioUrl { get; set; }

    public string? VideoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Category Category { get; set; } = null!;

    public QuestionType QuestionType { get; set; }
    = QuestionType.Standard;

    public string? MetadataJson { get; set; }
}