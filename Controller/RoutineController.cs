using FitnessPT_api.Dtos;
using FitnessPT_api.Models;
using FitnessPT_api.Repository;
using FitnessPT_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessPT_api.Controller;

[Route("api/routine")]
[ApiController]
public class RoutineController : ControllerBase
{
    private IRoutineRepository repository;
    private IDateTimeService timeService;
    private readonly ILogger<RoutineController> logger;

    public RoutineController(IRoutineRepository _repository, IDateTimeService _service,
        ILogger<RoutineController> _logger)
    {
        repository = _repository;
        timeService = _service;
        logger = _logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<RoutineDto>>> GetAllRoutine(int page = 1, int pageSize = 20, int userid = 0,
        string? level = null, string? category = null)
    {
        try
        {
            var result = await repository.GetAllRoutinesAsync(page, pageSize, userid);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "루틴 리스트 반환 오류");
            return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoutineInfoDto>> GetByIdAsync(int id)
    {
        try
        {
            var routine = await repository.GetByIdAsync(id);
            if (routine == null)
                return NotFound(new { message = "루틴을 찾을 수 없습니다." });

            var routineExercises = await repository.GetRoutineDetail(routine.Id);

            return new RoutineInfoDto()
            {
                Id = routine.Id,
                Name = routine.Name,
                Description = routine.Description,
                Level = routine.Level,
                Category = routine.Category,
                EstimatedDuration = routine.EstimatedDuration,
                ExerciseInfo = routineExercises.Select(re => new RoutineExerciseDto
                {
                    Id = re.Id,
                    RoutineId = re.RoutineId,
                    ExerciseId = re.ExerciseId,
                    OrderIndex = re.OrderIndex,
                    Sets = re.Sets,
                    Reps = re.Reps,
                    DurationSeconds = re.DurationSeconds,
                    RestSeconds = re.RestSeconds
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "루틴 리스트 검색 오류");
            return StatusCode(500, new { message = "서버 오류가 발생했습니다.", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<RoutineDto>> AddRoutine(RoutineInfoDto entity)
    {
        if (!Enum.TryParse<Level>(entity.Level, true, out var level))
            return BadRequest(new { message = "유효하지 않은 레벨입니다." });

        if (!Enum.TryParse<Category>(entity.Category, true, out var category))
            return BadRequest(new { message = "유효하지 않은 카테고리입니다." });

        var routine = new Routine()
        {
            Name = entity.Name,
            Description = entity.Description,
            Level = entity.Level,
            Category = entity.Category,
            EstimatedDuration = entity.EstimatedDuration,
            ThumbnailUrl = entity.ThumbnailUrl,
            CreatedUser = entity.CreateUser
        };

        var routineResult = await repository.AddAsync(routine);

        bool result = await repository.AddRoutineExercise(routineResult.Id, entity.ExerciseInfo);

        if (result)
            return Ok(routineResult);
        else
            return StatusCode(500, new { message = "루틴 리스트 추가 오류가 발생하였습니다." });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RoutineDto>> UpdateRoutine(int id, RoutineDto entity)
    {
        var existingRoutine = await repository.GetByIdAsync(id);
        if (existingRoutine == null)
            return NotFound(new { message = "루틴을 찾을 수 없습니다." });

        existingRoutine.Name = entity.Name;
        existingRoutine.Description = entity.Description;
        existingRoutine.Level = entity.Level;
        existingRoutine.Category = entity.Category;
        existingRoutine.EstimatedDuration = entity.EstimatedDuration;
        existingRoutine.ThumbnailUrl = entity.ThumbnailUrl;
        existingRoutine.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(existingRoutine);
        return Ok(existingRoutine);
    }

    [HttpPost("{routineId}/exercises")]
    public async Task<ActionResult> AddExerciseToRoutine(int routineId, RoutineExerciseDto exercise)
    {
        var routine = await repository.GetByIdAsync(routineId);
        if (routine == null)
            return NotFound(new { message = "루틴을 찾을 수 없습니다." });

        exercise.RoutineId = routineId;
        await repository.AddRoutineExercise(exercise);
        return Ok(new { message = "운동이 추가되었습니다." });
    }

    [HttpPut("{routineId}/exercises/{exerciseId}")]
    public async Task<ActionResult> UpdateRoutineExercise(
        int id,
        RoutineExerciseDto exercise)
    {
        var existing = await repository.UpdateRoutineExerciseById(id, exercise);
        return Ok(new { message = "운동이 수정되었습니다." });
    }

    [HttpDelete("{routineId}/exercises/{exerciseId}")]
    public async Task<ActionResult> DeleteRoutineExercise(int routineId, int exerciseId)
    {
        await repository.DeleteRoutineExerciseById(exerciseId);
        return Ok(new { message = "운동이 삭제되었습니다." });
    }
}