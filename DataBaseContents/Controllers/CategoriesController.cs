using FitnessPT_api.Data;
using FitnessPT_api.DataBaseContents.Dtos;
using FitnessPT_api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessPT_api.DataBaseContents.Controllers;

// Controllers/CategoriesController.cs
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(FitnessDbContext context, ILogger<CategoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 모든 카테고리 조회 (계층 구조)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories([FromQuery] bool includeExercises = false)
    {
        try
        {
            var query = _context.Exercisecategories
                .Include(c => c.InverseParentCategory)
                .AsQueryable();

            if (includeExercises)
                query = query.Include(c => c.Exercises.Where(e => e.IsActive == true));

            var categories = await query
                .Where(c => c.ParentCategoryId == null) // 최상위 카테고리만
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    ParentCategoryId = c.ParentCategoryId,
                    CategoryName = c.CategoryName,
                    CategoryCode = c.CategoryCode,
                    DisplayOrder = c.DisplayOrder,
                    SubCategories = c.InverseParentCategory
                        .OrderBy(sub => sub.DisplayOrder)
                        .ThenBy(sub => sub.CategoryName)
                        .Select(sub => new CategoryDto
                        {
                            CategoryId = sub.CategoryId,
                            ParentCategoryId = sub.ParentCategoryId,
                            CategoryName = sub.CategoryName,
                            CategoryCode = sub.CategoryCode,
                            DisplayOrder = sub.DisplayOrder,
                            Exercises = includeExercises
                                ? sub.Exercises
                                    .Where(e => e.IsActive == true)
                                    .Select(e => new ExerciseDto
                                    {
                                        ExerciseId = e.ExerciseId,
                                        ExerciseName = e.ExerciseName,
                                        DifficultyLevel = e.DifficultyLevel,
                                        DifficultyName = e.DifficultyLevel.GetDifficultyName(),
                                        TargetMuscles = e.TargetMuscles,
                                        IsActive = e.IsActive
                                    }).ToList()
                                : null
                        }).ToList(),
                    Exercises = includeExercises
                        ? c.Exercises
                            .Where(e => e.IsActive == true)
                            .Select(e => new ExerciseDto
                            {
                                ExerciseId = e.ExerciseId,
                                ExerciseName = e.ExerciseName,
                                DifficultyLevel = e.DifficultyLevel,
                                DifficultyName = e.DifficultyLevel.GetDifficultyName(),
                                TargetMuscles = e.TargetMuscles,
                                IsActive = e.IsActive
                            }).ToList()
                        : null
                })
                .ToListAsync();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "카테고리 목록 조회 중 오류 발생");
            return StatusCode(500, "카테고리 목록을 불러오는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 플랫 카테고리 목록 조회 (계층 구조 없이)
    /// </summary>
    [HttpGet("flat")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetFlatCategories()
    {
        try
        {
            var categories = await _context.Exercisecategories
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    ParentCategoryId = c.ParentCategoryId,
                    CategoryName = c.CategoryName,
                    CategoryCode = c.CategoryCode,
                    DisplayOrder = c.DisplayOrder
                })
                .ToListAsync();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "플랫 카테고리 목록 조회 중 오류 발생");
            return StatusCode(500, "카테고리 목록을 불러오는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 특정 카테고리 상세 조회
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id, [FromQuery] bool includeExercises = false)
    {
        try
        {
            var query = _context.Exercisecategories
                .Include(c => c.InverseParentCategory)
                .AsQueryable();

            if (includeExercises)
                query = query.Include(c => c.Exercises.Where(e => e.IsActive == true));

            var category = await query.FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound($"ID {id}에 해당하는 카테고리를 찾을 수 없습니다.");

            var categoryDto = new CategoryDto
            {
                CategoryId = category.CategoryId,
                ParentCategoryId = category.ParentCategoryId,
                CategoryName = category.CategoryName,
                CategoryCode = category.CategoryCode,
                DisplayOrder = category.DisplayOrder,
                SubCategories = category.InverseParentCategory
                    .OrderBy(sub => sub.DisplayOrder)
                    .ThenBy(sub => sub.CategoryName)
                    .Select(sub => new CategoryDto
                    {
                        CategoryId = sub.CategoryId,
                        ParentCategoryId = sub.ParentCategoryId,
                        CategoryName = sub.CategoryName,
                        CategoryCode = sub.CategoryCode,
                        DisplayOrder = sub.DisplayOrder
                    }).ToList(),
                Exercises = includeExercises
                    ? category.Exercises
                        .Where(e => e.IsActive == true)
                        .Select(e => new ExerciseDto
                        {
                            ExerciseId = e.ExerciseId,
                            ExerciseName = e.ExerciseName,
                            DifficultyLevel = e.DifficultyLevel,
                            DifficultyName = e.DifficultyLevel.GetDifficultyName(),
                            TargetMuscles = e.TargetMuscles,
                            Instructions = e.Instructions,
                            IsActive = e.IsActive,
                            CreatedAt = e.CreatedAt
                        }).ToList()
                    : null
            };

            return Ok(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "카테고리 상세 조회 중 오류 발생 - ID: {CategoryId}", id);
            return StatusCode(500, "카테고리 정보를 불러오는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 새 카테고리 생성
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createDto)
    {
        try
        {
            // 부모 카테고리 존재 확인
            if (createDto.ParentCategoryId.HasValue)
            {
                var parentExists = await _context.Exercisecategories
                    .AnyAsync(c => c.CategoryId == createDto.ParentCategoryId);
                if (!parentExists)
                    return BadRequest("존재하지 않는 부모 카테고리입니다.");
            }

            // 카테고리 코드 중복 확인
            var codeExists = await _context.Exercisecategories
                .AnyAsync(c => c.CategoryCode == createDto.CategoryCode);
            if (codeExists)
                return BadRequest("이미 존재하는 카테고리 코드입니다.");

            var category = new Exercisecategory
            {
                ParentCategoryId = createDto.ParentCategoryId,
                CategoryName = createDto.CategoryName,
                CategoryCode = createDto.CategoryCode,
                DisplayOrder = createDto.DisplayOrder
            };

            _context.Exercisecategories.Add(category);
            await _context.SaveChangesAsync();

            var categoryDto = new CategoryDto
            {
                CategoryId = category.CategoryId,
                ParentCategoryId = category.ParentCategoryId,
                CategoryName = category.CategoryName,
                CategoryCode = category.CategoryCode,
                DisplayOrder = category.DisplayOrder
            };

            return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryId }, categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "카테고리 생성 중 오류 발생");
            return StatusCode(500, "카테고리를 생성하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 카테고리 정보 수정
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, UpdateCategoryDto updateDto)
    {
        try
        {
            var category = await _context.Exercisecategories.FindAsync(id);
            if (category == null)
                return NotFound($"ID {id}에 해당하는 카테고리를 찾을 수 없습니다.");

            // 부모 카테고리 존재 확인 및 순환 참조 방지
            if (updateDto.ParentCategoryId.HasValue)
            {
                if (updateDto.ParentCategoryId == id)
                    return BadRequest("자기 자신을 부모 카테고리로 설정할 수 없습니다.");

                var parentExists = await _context.Exercisecategories
                    .AnyAsync(c => c.CategoryId == updateDto.ParentCategoryId);
                if (!parentExists)
                    return BadRequest("존재하지 않는 부모 카테고리입니다.");

                // 하위 카테고리가 부모가 되는 순환 참조 방지
                var isDescendant = await IsDescendantCategory(id, updateDto.ParentCategoryId.Value);
                if (isDescendant)
                    return BadRequest("하위 카테고리를 부모 카테고리로 설정할 수 없습니다.");
            }

            // 카테고리 코드 중복 확인
            if (!string.IsNullOrEmpty(updateDto.CategoryCode) && updateDto.CategoryCode != category.CategoryCode)
            {
                var codeExists = await _context.Exercisecategories
                    .AnyAsync(c => c.CategoryCode == updateDto.CategoryCode && c.CategoryId != id);
                if (codeExists)
                    return BadRequest("이미 존재하는 카테고리 코드입니다.");
            }

            // 업데이트
            if (updateDto.ParentCategoryId.HasValue)
                category.ParentCategoryId = updateDto.ParentCategoryId;
            if (!string.IsNullOrEmpty(updateDto.CategoryName))
                category.CategoryName = updateDto.CategoryName;
            if (!string.IsNullOrEmpty(updateDto.CategoryCode))
                category.CategoryCode = updateDto.CategoryCode;
            if (updateDto.DisplayOrder.HasValue)
                category.DisplayOrder = updateDto.DisplayOrder.Value;

            await _context.SaveChangesAsync();

            var categoryDto = new CategoryDto
            {
                CategoryId = category.CategoryId,
                ParentCategoryId = category.ParentCategoryId,
                CategoryName = category.CategoryName,
                CategoryCode = category.CategoryCode,
                DisplayOrder = category.DisplayOrder
            };

            return Ok(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "카테고리 수정 중 오류 발생 - ID: {CategoryId}", id);
            return StatusCode(500, "카테고리 정보를 수정하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 카테고리 삭제
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            var category = await _context.Exercisecategories
                .Include(c => c.InverseParentCategory)
                .Include(c => c.Exercises)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound($"ID {id}에 해당하는 카테고리를 찾을 수 없습니다.");

            // 하위 카테고리가 있는지 확인
            if (category.InverseParentCategory.Any())
                return BadRequest("하위 카테고리가 있는 카테고리는 삭제할 수 없습니다.");

            // 연결된 운동이 있는지 확인
            if (category.Exercises.Any())
                return BadRequest("연결된 운동이 있는 카테고리는 삭제할 수 없습니다.");

            _context.Exercisecategories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "카테고리가 삭제되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "카테고리 삭제 중 오류 발생 - ID: {CategoryId}", id);
            return StatusCode(500, "카테고리를 삭제하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 카테고리 순서 재정렬
    /// </summary>
    [HttpPut("reorder")]
    public async Task<IActionResult> ReorderCategories([FromBody] List<ReorderCategoryDto> reorderList)
    {
        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            foreach (var item in reorderList)
            {
                var category = await _context.Exercisecategories.FindAsync(item.CategoryId);
                if (category != null)
                {
                    category.DisplayOrder = item.DisplayOrder;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "카테고리 순서가 변경되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "카테고리 순서 변경 중 오류 발생");
            return StatusCode(500, "카테고리 순서를 변경하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 카테고리별 운동 개수 조회
    /// </summary>
    [HttpGet("exercise-counts")]
    public async Task<ActionResult> GetCategoryExerciseCounts()
    {
        try
        {
            var counts = await _context.Exercisecategories
                .Select(c => new
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryCode = c.CategoryCode,
                    ExerciseCount = c.Exercises.Count(e => e.IsActive == true),
                    ActiveExerciseCount = c.Exercises.Count(e => e.IsActive == true),
                    InactiveExerciseCount = c.Exercises.Count(e => e.IsActive == false)
                })
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return Ok(counts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "카테고리별 운동 개수 조회 중 오류 발생");
            return StatusCode(500, "카테고리별 운동 개수를 불러오는 중 오류가 발생했습니다.");
        }
    }

    // 순환 참조 확인 헬퍼 메서드
    private async Task<bool> IsDescendantCategory(int parentId, int childId)
    {
        var descendants = await GetAllDescendants(parentId);
        return descendants.Contains(childId);
    }

    private async Task<List<int>> GetAllDescendants(int categoryId)
    {
        var descendants = new List<int>();
        var directChildren = await _context.Exercisecategories
            .Where(c => c.ParentCategoryId == categoryId)
            .Select(c => c.CategoryId)
            .ToListAsync();

        descendants.AddRange(directChildren);

        foreach (var childId in directChildren)
        {
            var childDescendants = await GetAllDescendants(childId);
            descendants.AddRange(childDescendants);
        }

        return descendants;
    }
}

// 순서 재정렬용 DTO
public class ReorderCategoryDto
{
    public int CategoryId { get; set; }
    public int DisplayOrder { get; set; }
}