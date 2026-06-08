using Microsoft.AspNetCore.Mvc;
using SeenJeemGame.Application.Questions;
using SeenJeemGame.Application.Questions.Dtos;

namespace SeenJeemGame.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuestionDto>>> GetAll()
    {
        var questions = await _questionService.GetAllAsync();

        return Ok(questions);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<QuestionDto>> GetById(int id)
    {
        var question = await _questionService.GetByIdAsync(id);

        if (question is null)
            return NotFound();

        return Ok(question);
    }

    [HttpGet("by-category/{categoryId:int}")]
    public async Task<ActionResult<List<QuestionDto>>> GetByCategoryId(int categoryId)
    {
        var questions = await _questionService.GetByCategoryIdAsync(categoryId);

        return Ok(questions);
    }

    [HttpPost]
    public async Task<ActionResult<QuestionDto>> Create(CreateQuestionRequest request)
    {
        try
        {
            var question = await _questionService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = question.Id },
                question);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateQuestionRequest request)
    {
        try
        {
            var updated = await _questionService.UpdateAsync(id, request);

            if (!updated)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _questionService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<BulkCreateQuestionsResponse>> BulkCreate(
    List<CreateQuestionRequest> requests)
    {
        var result = await _questionService.BulkCreateAsync(requests);

        if (result.ErrorCount > 0)
            return BadRequest(result);

        return Ok(result);
    }
}