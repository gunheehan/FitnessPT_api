using FitnessPT_api.Dtos;
using FitnessPT_api.Models;
using FitnessPT_api.Repository;
using FitnessPT_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessPT_api.Controller;

[Route("api/exercises")]
[ApiController]
public class ExerciseController : ControllerBase
{
    private IExerciseRepository repository;
    private IDateTimeService timeService;

    public ExerciseController(IExerciseRepository _repository, IDateTimeService _timeService)
    {
        repository = _repository;
        timeService = _timeService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ExerciseDto>>> GetAllExercise(int page = 1, int pageSize = 20, string? level = null,
        string? category = null)
    {
        try
        {
            if (page < 1)
                return BadRequest(new { message = "페이지는 1 이상이어야 합니다." });

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(new { message = "페이지 크기는 1-100 사이여야 합니다." });

            var result = await repository.GetAllExerciseAsync(page, pageSize, level, category);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseDto>> GetByIdAsync(int id)
    {
        var exercise = await repository.GetByIdAsync(id);
        
        if (exercise == null)
            return NotFound(new { message = "운동을 찾을 수 없습니다." });

        // Entity → DTO 변환
        var dto = new ExerciseDto
        {
            Id = exercise.Id,
            Name = exercise.Name,
            Description = exercise.Description,
            Level = exercise.Level,
            Category = exercise.Category,
            ImageUrl = exercise.ImageUrl,
            VideoUrl = exercise.VideoUrl
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ExerciseDto>> AddExercise(ExerciseDto entity)
    {
        try
        {
            if (!Enum.TryParse<Level>(entity.Level, true, out var level))
                return BadRequest(new { message = "유효하지 않은 레벨입니다." });

            if (!Enum.TryParse<Category>(entity.Category, true, out var category))
                return BadRequest(new { message = "유효하지 않은 카테고리입니다." });
            
            // Entity 생성
            var exercise = new Exercise
            {
                Name = entity.Name,
                Description = entity.Description,
                Level = entity.Level,
                Category = entity.Category,
                CategoryDetail = entity.CategoryDetail,
                ImageUrl = entity.ImageUrl,
                VideoUrl = entity.VideoUrl,
                CreatedAt = timeService.UtcNow,
                UpdatedAt = timeService.UtcNow
            };

            var result = await repository.AddAsync(exercise);

            // DTO로 변환
            var responseDto = new ExerciseDto
            {
                Id = result.Id,
                Name = result.Name,
                Description = result.Description,
                Level = result.Level,
                Category = result.Category,
                ImageUrl = result.ImageUrl,
                VideoUrl = result.VideoUrl
            };

            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ExerciseDto>> UpdateExercice(int id, ExerciseDto entity)
    {
        try
        {
            var exercise = await repository.GetByIdAsync(id);
            if (exercise == null)
                return NotFound(new { message = "운동을 찾을 수 없습니다." });

            exercise.UpdatedAt = DateTime.UtcNow;
            await repository.UpdateAsync(exercise);

            var responseDto = new ExerciseDto
            {
                Id = exercise.Id,
                Name = exercise.Name,
                Description = exercise.Description,
                Level = exercise.Level,
                Category = exercise.Category,
                ImageUrl = exercise.ImageUrl,
                VideoUrl = exercise.VideoUrl
            };

            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        try
        {
            var exercise = await repository.GetByIdAsync(id);
            if (exercise == null)
                return NotFound(new { message = "운동을 찾을 수 없습니다." });

            await repository.DeleteAsync(id);
            
            return NoContent();  // 204 No Content
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
        }
    }
}