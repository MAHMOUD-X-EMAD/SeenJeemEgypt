using SeenJeemGame.Application.Questions.Dtos;

namespace SeenJeemGame.Application.Questions;

public interface IQuestionService
{
    Task<List<QuestionDto>> GetAllAsync();

    Task<QuestionDto?> GetByIdAsync(int id);

    Task<List<QuestionDto>> GetByCategoryIdAsync(int categoryId);

    Task<QuestionDto> CreateAsync(CreateQuestionRequest request);

    Task<BulkCreateQuestionsResponse> BulkCreateAsync(List<CreateQuestionRequest> requests);

    Task<bool> UpdateAsync(int id, UpdateQuestionRequest request);

    Task<bool> DeleteAsync(int id);
}