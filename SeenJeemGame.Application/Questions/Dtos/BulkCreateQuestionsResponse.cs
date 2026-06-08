namespace SeenJeemGame.Application.Questions.Dtos;

public class BulkCreateQuestionsResponse
{
    public int TotalReceived { get; set; }

    public int InsertedCount { get; set; }

    public int SkippedDuplicatesCount { get; set; }

    public int ErrorCount { get; set; }

    public List<string> Errors { get; set; } = new();
}