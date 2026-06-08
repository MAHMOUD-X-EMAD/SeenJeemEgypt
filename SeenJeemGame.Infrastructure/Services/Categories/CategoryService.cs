using Microsoft.EntityFrameworkCore;
using SeenJeemGame.Application.Categories;
using SeenJeemGame.Application.Categories.Dtos;
using SeenJeemGame.Domain.Entities;
using SeenJeemGame.Infrastructure.Persistence;

namespace SeenJeemGame.Infrastructure.Services.Categories;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _dbContext;

    public CategoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive,
                QuestionsCount = x.Questions.Count(q => q.IsActive)
            })
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive,
                QuestionsCount = x.Questions.Count(q => q.IsActive)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
    {
        var name = request.Name.Trim();

        var exists = await _dbContext.Categories
            .AnyAsync(x => x.Name == name);

        if (exists)
            throw new InvalidOperationException("Category name already exists.");

        var category = new Category
        {
            Name = name,
            Description = request.Description?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            IsActive = category.IsActive,
            QuestionsCount = 0
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (category is null)
            return false;

        var name = request.Name.Trim();

        var duplicateName = await _dbContext.Categories
            .AnyAsync(x => x.Id != id && x.Name == name);

        if (duplicateName)
            throw new InvalidOperationException("Category name already exists.");

        category.Name = name;
        category.Description = request.Description?.Trim();
        category.ImageUrl = request.ImageUrl?.Trim();
        category.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (category is null)
            return false;

        category.IsActive = false;

        await _dbContext.SaveChangesAsync();

        return true;
    }
    public async Task<BulkCreateCategoriesResponse> BulkCreateAsync(
    List<CreateCategoryRequest> requests)
    {
        var response = new BulkCreateCategoriesResponse
        {
            TotalReceived = requests?.Count ?? 0
        };

        if (requests is null || requests.Count == 0)
        {
            response.Errors.Add("Categories list is required.");
            response.ErrorCount = response.Errors.Count;
            return response;
        }

        var normalizedNames = requests
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => x.Name.Trim().ToLower())
            .Distinct()
            .ToList();

        var existingNames = await _dbContext.Categories
            .Where(x => normalizedNames.Contains(x.Name.ToLower()))
            .Select(x => x.Name.ToLower())
            .ToListAsync();

        var payloadNames = new HashSet<string>();
        var categoriesToInsert = new List<Category>();

        for (var i = 0; i < requests.Count; i++)
        {
            var request = requests[i];

            if (request is null)
            {
                response.Errors.Add($"Row {i + 1}: Category object is null.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                response.Errors.Add($"Row {i + 1}: Category name is required.");
                continue;
            }

            var trimmedName = request.Name.Trim();
            var normalizedName = trimmedName.ToLower();

            if (existingNames.Contains(normalizedName))
            {
                response.SkippedDuplicatesCount++;
                response.SkippedDuplicateNames.Add(trimmedName);
                continue;
            }

            if (payloadNames.Contains(normalizedName))
            {
                response.SkippedDuplicatesCount++;
                response.SkippedDuplicateNames.Add(trimmedName);
                continue;
            }

            payloadNames.Add(normalizedName);

            categoriesToInsert.Add(new Category
            {
                Name = trimmedName,
                Description = request.Description?.Trim(),
                ImageUrl = request.ImageUrl?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        response.ErrorCount = response.Errors.Count;

        if (categoriesToInsert.Count == 0)
        {
            return response;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        _dbContext.Categories.AddRange(categoriesToInsert);

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        response.InsertedCount = categoriesToInsert.Count;

        response.InsertedCategories = categoriesToInsert
            .OrderBy(x => x.Id)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive,
                QuestionsCount = 0
            })
            .ToList();

        return response;
    }
}