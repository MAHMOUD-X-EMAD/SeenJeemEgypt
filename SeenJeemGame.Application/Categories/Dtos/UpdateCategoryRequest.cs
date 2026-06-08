namespace SeenJeemGame.Application.Categories.Dtos;

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
}