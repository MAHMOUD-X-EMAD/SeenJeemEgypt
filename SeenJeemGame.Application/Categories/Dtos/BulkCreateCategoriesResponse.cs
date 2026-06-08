namespace SeenJeemGame.Application.Categories.Dtos;

public class BulkCreateCategoriesResponse
{
    public int TotalReceived { get; set; }

    public int InsertedCount { get; set; }

    public int SkippedDuplicatesCount { get; set; }

    public int ErrorCount { get; set; }

    public List<string> Errors { get; set; } = new();

    public List<CategoryDto> InsertedCategories { get; set; } = new();

    public List<string> SkippedDuplicateNames { get; set; } = new();
}