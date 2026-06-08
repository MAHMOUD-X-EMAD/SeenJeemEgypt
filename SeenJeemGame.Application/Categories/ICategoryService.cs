using SeenJeemGame.Application.Categories.Dtos;

namespace SeenJeemGame.Application.Categories;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();

    Task<CategoryDto?> GetByIdAsync(int id);

    Task<CategoryDto> CreateAsync(CreateCategoryRequest request);

    Task<BulkCreateCategoriesResponse> BulkCreateAsync(
        List<CreateCategoryRequest> requests);

    Task<bool> UpdateAsync(int id, UpdateCategoryRequest request);

    Task<bool> DeleteAsync(int id);
}