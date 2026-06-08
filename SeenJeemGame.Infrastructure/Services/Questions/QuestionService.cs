using Microsoft.EntityFrameworkCore;
using SeenJeemGame.Application.Questions;
using SeenJeemGame.Application.Questions.Dtos;
using SeenJeemGame.Domain.Entities;
using SeenJeemGame.Domain.Enums;
using SeenJeemGame.Infrastructure.Persistence;

namespace SeenJeemGame.Infrastructure.Services.Questions;

public class QuestionService : IQuestionService
{
    private readonly AppDbContext _dbContext;

    public QuestionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<QuestionDto>> GetAllAsync()
    {
        return await _dbContext.Questions
            .AsNoTracking()
            .OrderBy(x => x.CategoryId)
            .ThenBy(x => x.Difficulty)
            .ThenBy(x => x.Id)
            .Select(x => new QuestionDto
            {
                Id = x.Id,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                Text = x.Text,
                CorrectAnswer = x.CorrectAnswer,
                Difficulty = x.Difficulty,
                Points = x.Points,
                ImageUrl = x.ImageUrl,
                AudioUrl = x.AudioUrl,
                VideoUrl = x.VideoUrl,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<QuestionDto?> GetByIdAsync(int id)
    {
        return await _dbContext.Questions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new QuestionDto
            {
                Id = x.Id,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                Text = x.Text,
                CorrectAnswer = x.CorrectAnswer,
                Difficulty = x.Difficulty,
                Points = x.Points,
                ImageUrl = x.ImageUrl,
                AudioUrl = x.AudioUrl,
                VideoUrl = x.VideoUrl,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<QuestionDto>> GetByCategoryIdAsync(int categoryId)
    {
        return await _dbContext.Questions
            .AsNoTracking()
            .Where(x => x.CategoryId == categoryId)
            .OrderBy(x => x.Difficulty)
            .ThenBy(x => x.Id)
            .Select(x => new QuestionDto
            {
                Id = x.Id,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                Text = x.Text,
                CorrectAnswer = x.CorrectAnswer,
                Difficulty = x.Difficulty,
                Points = x.Points,
                ImageUrl = x.ImageUrl,
                AudioUrl = x.AudioUrl,
                VideoUrl = x.VideoUrl,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<QuestionDto> CreateAsync(CreateQuestionRequest request)
    {
        var categoryExists = await _dbContext.Categories
            .AnyAsync(x => x.Id == request.CategoryId && x.IsActive);

        if (!categoryExists)
            throw new InvalidOperationException("Category does not exist or is not active.");

        ValidateQuestion(request.Text, request.CorrectAnswer, request.Difficulty);

        var question = new Question
        {
            CategoryId = request.CategoryId,
            Text = request.Text.Trim(),
            CorrectAnswer = request.CorrectAnswer.Trim(),
            Difficulty = request.Difficulty,
            Points = GetPointsByDifficulty(request.Difficulty),
            ImageUrl = request.ImageUrl?.Trim(),
            AudioUrl = request.AudioUrl?.Trim(),
            VideoUrl = request.VideoUrl?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Questions.Add(question);
        await _dbContext.SaveChangesAsync();

        var categoryName = await _dbContext.Categories
            .Where(x => x.Id == question.CategoryId)
            .Select(x => x.Name)
            .FirstAsync();

        return new QuestionDto
        {
            Id = question.Id,
            CategoryId = question.CategoryId,
            CategoryName = categoryName,
            Text = question.Text,
            CorrectAnswer = question.CorrectAnswer,
            Difficulty = question.Difficulty,
            Points = question.Points,
            ImageUrl = question.ImageUrl,
            AudioUrl = question.AudioUrl,
            VideoUrl = question.VideoUrl,
            IsActive = question.IsActive
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateQuestionRequest request)
    {
        var question = await _dbContext.Questions
            .FirstOrDefaultAsync(x => x.Id == id);

        if (question is null)
            return false;

        var categoryExists = await _dbContext.Categories
            .AnyAsync(x => x.Id == request.CategoryId && x.IsActive);

        if (!categoryExists)
            throw new InvalidOperationException("Category does not exist or is not active.");

        ValidateQuestion(request.Text, request.CorrectAnswer, request.Difficulty);

        question.CategoryId = request.CategoryId;
        question.Text = request.Text.Trim();
        question.CorrectAnswer = request.CorrectAnswer.Trim();
        question.Difficulty = request.Difficulty;
        question.Points = GetPointsByDifficulty(request.Difficulty);
        question.ImageUrl = request.ImageUrl?.Trim();
        question.AudioUrl = request.AudioUrl?.Trim();
        question.VideoUrl = request.VideoUrl?.Trim();
        question.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var question = await _dbContext.Questions
            .FirstOrDefaultAsync(x => x.Id == id);

        if (question is null)
            return false;

        question.IsActive = false;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static int GetPointsByDifficulty(QuestionDifficulty difficulty)
    {
        return difficulty switch
        {
            QuestionDifficulty.Easy => 200,
            QuestionDifficulty.Medium => 400,
            QuestionDifficulty.Hard => 600,
            _ => throw new InvalidOperationException("Invalid difficulty.")
        };
    }

    private static void ValidateQuestion(
        string text,
        string correctAnswer,
        QuestionDifficulty difficulty)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Question text is required.");

        if (string.IsNullOrWhiteSpace(correctAnswer))
            throw new InvalidOperationException("Correct answer is required.");

        if (!Enum.IsDefined(typeof(QuestionDifficulty), difficulty))
            throw new InvalidOperationException("Invalid question difficulty.");
    }

    public async Task<BulkCreateQuestionsResponse> BulkCreateAsync(List<CreateQuestionRequest> requests)
    {
        var response = new BulkCreateQuestionsResponse
        {
            TotalReceived = requests?.Count ?? 0
        };

        if (requests is null || requests.Count == 0)
        {
            response.Errors.Add("Questions list is empty.");
            response.ErrorCount = response.Errors.Count;
            return response;
        }

        var validCategoryIds = await _dbContext.Categories
            .Where(x => x.IsActive)
            .Select(x => x.Id)
            .ToListAsync();

        var validCategoryIdsSet = validCategoryIds.ToHashSet();

        var existingQuestions = await _dbContext.Questions
            .Select(x => new
            {
                x.CategoryId,
                NormalizedText = x.Text.ToLower()
            })
            .ToListAsync();

        var existingSet = existingQuestions
            .Select(x => $"{x.CategoryId}|{x.NormalizedText.Trim()}")
            .ToHashSet();

        var newQuestionsSet = new HashSet<string>();

        var questionsToInsert = new List<Question>();

        for (var i = 0; i < requests.Count; i++)
        {
            var request = requests[i];
            var rowNumber = i + 1;

            if (request is null)
            {
                response.Errors.Add($"Row {rowNumber}: Question object is null.");
                continue;
            }

            if (!validCategoryIdsSet.Contains(request.CategoryId))
            {
                response.Errors.Add($"Row {rowNumber}: CategoryId {request.CategoryId} does not exist or is not active.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(request.Text))
            {
                response.Errors.Add($"Row {rowNumber}: Question text is required.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(request.CorrectAnswer))
            {
                response.Errors.Add($"Row {rowNumber}: Correct answer is required.");
                continue;
            }

            if (!Enum.IsDefined(typeof(QuestionDifficulty), request.Difficulty))
            {
                response.Errors.Add($"Row {rowNumber}: Invalid difficulty value.");
                continue;
            }

            var normalizedText = request.Text.Trim().ToLower();
            var duplicateKey = $"{request.CategoryId}|{normalizedText}";

            if (existingSet.Contains(duplicateKey) || newQuestionsSet.Contains(duplicateKey))
            {
                response.SkippedDuplicatesCount++;
                continue;
            }

            newQuestionsSet.Add(duplicateKey);

            questionsToInsert.Add(new Question
            {
                CategoryId = request.CategoryId,
                Text = request.Text.Trim(),
                CorrectAnswer = request.CorrectAnswer.Trim(),
                Difficulty = request.Difficulty,
                Points = GetPointsByDifficulty(request.Difficulty),
                ImageUrl = request.ImageUrl?.Trim(),
                AudioUrl = request.AudioUrl?.Trim(),
                VideoUrl = request.VideoUrl?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        response.ErrorCount = response.Errors.Count;

        if (response.ErrorCount > 0)
        {
            return response;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        _dbContext.Questions.AddRange(questionsToInsert);
        await _dbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        response.InsertedCount = questionsToInsert.Count;

        return response;
    }
}