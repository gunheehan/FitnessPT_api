using FitnessPT_api.Data;
using FitnessPT_api.DataBaseContents.Dtos;
using FitnessPT_api.Models;

namespace FitnessPT_api.DataBaseContents.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ExercisesController : ControllerBase
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<ExercisesController> _logger;

    public ExercisesController(FitnessDbContext context, ILogger<ExercisesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 모든 운동 목록 조회 (필터링 지원)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExerciseDto>>> GetExercises(
        [FromQuery] int? categoryId = null,
        [FromQuery] int? difficultyLevel = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _context.Exercises
                .Include(e => e.PrimaryCategory)
                .AsQueryable();

            // 필터링
            if (categoryId.HasValue)
                query = query.Where(e => e.PrimaryCategoryId == categoryId);

            if (difficultyLevel.HasValue)
                query = query.Where(e => e.DifficultyLevel == difficultyLevel);

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(e => e.ExerciseName.Contains(searchTerm) ||
                                         (e.TargetMuscles != null && e.TargetMuscles.Contains(searchTerm)));

            if (isActive.HasValue)
                query = query.Where(e => e.IsActive == isActive);

            // 페이징
            var totalCount = await query.CountAsync();
            var exercises = await query
                .OrderBy(e => e.ExerciseName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new ExerciseDto
                {
                    ExerciseId = e.ExerciseId,
                    ExerciseName = e.ExerciseName,
                    PrimaryCategoryId = e.PrimaryCategoryId,
                    CategoryName = e.PrimaryCategory != null ? e.PrimaryCategory.CategoryName : null,
                    CategoryCode = e.PrimaryCategory != null ? e.PrimaryCategory.CategoryCode : null,
                    DifficultyLevel = e.DifficultyLevel,
                    DifficultyName = e.DifficultyLevel.GetDifficultyName(),
                    TargetMuscles = e.TargetMuscles,
                    Instructions = e.Instructions,
                    IsActive = e.IsActive,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Page", page.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(exercises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "운동 목록 조회 중 오류 발생");
            return StatusCode(500, "운동 목록을 불러오는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 특정 운동 상세 조회
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseDto>> GetExercise(int id)
    {
        try
        {
            var exercise = await _context.Exercises
                .Include(e => e.PrimaryCategory)
                .FirstOrDefaultAsync(e => e.ExerciseId == id);

            if (exercise == null)
                return NotFound($"ID {id}에 해당하는 운동을 찾을 수 없습니다.");

            var exerciseDto = new ExerciseDto
            {
                ExerciseId = exercise.ExerciseId,
                ExerciseName = exercise.ExerciseName,
                PrimaryCategoryId = exercise.PrimaryCategoryId,
                CategoryName = exercise.PrimaryCategory?.CategoryName,
                CategoryCode = exercise.PrimaryCategory?.CategoryCode,
                DifficultyLevel = exercise.DifficultyLevel,
                DifficultyName = exercise.DifficultyLevel.GetDifficultyName(),
                TargetMuscles = exercise.TargetMuscles,
                Instructions = exercise.Instructions,
                IsActive = exercise.IsActive,
                CreatedAt = exercise.CreatedAt
            };

            return Ok(exerciseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "운동 상세 조회 중 오류 발생 - ID: {ExerciseId}", id);
            return StatusCode(500, "운동 정보를 불러오는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 새 운동 생성
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ExerciseDto>> CreateExercise(CreateExerciseDto createDto)
    {
        try
        {
            // 카테고리 존재 확인
            if (createDto.PrimaryCategoryId.HasValue)
            {
                var categoryExists = await _context.Exercisecategories
                    .AnyAsync(c => c.CategoryId == createDto.PrimaryCategoryId);
                if (!categoryExists)
                    return BadRequest("존재하지 않는 카테고리입니다.");
            }

            var exercise = new Exercise
            {
                ExerciseName = createDto.ExerciseName,
                PrimaryCategoryId = createDto.PrimaryCategoryId,
                DifficultyLevel = createDto.DifficultyLevel,
                TargetMuscles = createDto.TargetMuscles,
                Instructions = createDto.Instructions,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            // 생성된 운동 정보 반환
            var createdExercise = await _context.Exercises
                .Include(e => e.PrimaryCategory)
                .FirstAsync(e => e.ExerciseId == exercise.ExerciseId);

            var exerciseDto = new ExerciseDto
            {
                ExerciseId = createdExercise.ExerciseId,
                ExerciseName = createdExercise.ExerciseName,
                PrimaryCategoryId = createdExercise.PrimaryCategoryId,
                CategoryName = createdExercise.PrimaryCategory?.CategoryName,
                CategoryCode = createdExercise.PrimaryCategory?.CategoryCode,
                DifficultyLevel = createdExercise.DifficultyLevel,
                DifficultyName = createdExercise.DifficultyLevel.GetDifficultyName(),
                TargetMuscles = createdExercise.TargetMuscles,
                Instructions = createdExercise.Instructions,
                IsActive = createdExercise.IsActive,
                CreatedAt = createdExercise.CreatedAt
            };

            return CreatedAtAction(nameof(GetExercise), new { id = exercise.ExerciseId }, exerciseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "운동 생성 중 오류 발생");
            return StatusCode(500, "운동을 생성하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 운동 정보 수정
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ExerciseDto>> UpdateExercise(int id, UpdateExerciseDto updateDto)
    {
        try
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise == null)
                return NotFound($"ID {id}에 해당하는 운동을 찾을 수 없습니다.");

            // 카테고리 존재 확인
            if (updateDto.PrimaryCategoryId.HasValue)
            {
                var categoryExists = await _context.Exercisecategories
                    .AnyAsync(c => c.CategoryId == updateDto.PrimaryCategoryId);
                if (!categoryExists)
                    return BadRequest("존재하지 않는 카테고리입니다.");
            }

            // 업데이트
            if (!string.IsNullOrEmpty(updateDto.ExerciseName))
                exercise.ExerciseName = updateDto.ExerciseName;
            if (updateDto.PrimaryCategoryId.HasValue)
                exercise.PrimaryCategoryId = updateDto.PrimaryCategoryId;
            if (updateDto.DifficultyLevel.HasValue)
                exercise.DifficultyLevel = updateDto.DifficultyLevel;
            if (updateDto.TargetMuscles != null)
                exercise.TargetMuscles = updateDto.TargetMuscles;
            if (updateDto.Instructions != null)
                exercise.Instructions = updateDto.Instructions;
            if (updateDto.IsActive.HasValue)
                exercise.IsActive = updateDto.IsActive;

            await _context.SaveChangesAsync();

            // 업데이트된 정보 반환
            var updatedExercise = await _context.Exercises
                .Include(e => e.PrimaryCategory)
                .FirstAsync(e => e.ExerciseId == id);

            var exerciseDto = new ExerciseDto
            {
                ExerciseId = updatedExercise.ExerciseId,
                ExerciseName = updatedExercise.ExerciseName,
                PrimaryCategoryId = updatedExercise.PrimaryCategoryId,
                CategoryName = updatedExercise.PrimaryCategory?.CategoryName,
                CategoryCode = updatedExercise.PrimaryCategory?.CategoryCode,
                DifficultyLevel = updatedExercise.DifficultyLevel,
                DifficultyName = updatedExercise.DifficultyLevel.GetDifficultyName(),
                TargetMuscles = updatedExercise.TargetMuscles,
                Instructions = updatedExercise.Instructions,
                IsActive = updatedExercise.IsActive,
                CreatedAt = updatedExercise.CreatedAt
            };

            return Ok(exerciseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "운동 수정 중 오류 발생 - ID: {ExerciseId}", id);
            return StatusCode(500, "운동 정보를 수정하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 운동 삭제 (소프트 삭제 - IsActive를 false로 변경)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExercise(int id)
    {
        try
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise == null)
                return NotFound($"ID {id}에 해당하는 운동을 찾을 수 없습니다.");

            // 워크아웃 기록이 있는지 확인
            var hasWorkoutRecords = await _context.Workoutrecords
                .AnyAsync(w => w.ExerciseId == id);

            if (hasWorkoutRecords)
            {
                // 워크아웃 기록이 있으면 소프트 삭제
                exercise.IsActive = false;
                await _context.SaveChangesAsync();
                return Ok(new { message = "운동이 비활성화되었습니다. (워크아웃 기록 보존)" });
            }
            else
            {
                // 워크아웃 기록이 없으면 실제 삭제
                _context.Exercises.Remove(exercise);
                await _context.SaveChangesAsync();
                return Ok(new { message = "운동이 완전히 삭제되었습니다." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "운동 삭제 중 오류 발생 - ID: {ExerciseId}", id);
            return StatusCode(500, "운동을 삭제하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 운동 활성화/비활성화 토글
    /// </summary>
    [HttpPatch("{id}/toggle-status")]
    public async Task<IActionResult> ToggleExerciseStatus(int id)
    {
        try
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise == null)
                return NotFound($"ID {id}에 해당하는 운동을 찾을 수 없습니다.");

            exercise.IsActive = !exercise.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                exerciseId = id,
                isActive = exercise.IsActive,
                message = exercise.IsActive == true ? "운동이 활성화되었습니다." : "운동이 비활성화되었습니다."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "운동 상태 변경 중 오류 발생 - ID: {ExerciseId}", id);
            return StatusCode(500, "운동 상태를 변경하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 카테고리별 운동 통계
    /// </summary>
    [HttpGet("statistics/by-category")]
    public async Task<ActionResult> GetExerciseStatisticsByCategory()
    {
        try
        {
            var statistics = await _context.Exercises
                .Include(e => e.PrimaryCategory)
                .Where(e => e.IsActive == true)
                .GroupBy(e => new { e.PrimaryCategoryId, e.PrimaryCategory!.CategoryName })
                .Select(g => new
                {
                    CategoryId = g.Key.PrimaryCategoryId,
                    CategoryName = g.Key.CategoryName,
                    ExerciseCount = g.Count(),
                    DifficultyBreakdown = g.GroupBy(e => e.DifficultyLevel)
                        .Select(dg => new
                        {
                            DifficultyLevel = dg.Key,
                            DifficultyName = dg.Key.GetDifficultyName(),
                            Count = dg.Count()
                        }).ToList()
                })
                .OrderBy(s => s.CategoryName)
                .ToListAsync();

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "운동 통계 조회 중 오류 발생");
            return StatusCode(500, "운동 통계를 불러오는 중 오류가 발생했습니다.");
        }
    }
}