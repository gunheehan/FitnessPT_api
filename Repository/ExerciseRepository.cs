using FitnessPT_api.Data;
using FitnessPT_api.Dtos;
using FitnessPT_api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessPT_api.Repository;

public interface IExerciseRepository : IRepository<Exercise>
{
    Task<PagedResult<ExerciseDto>> GetAllExerciseAsync(int page = 1, int pageSize = 20, string? level = null, string? category = null);
}

public class ExerciseRepository : Repository<Exercise>, IExerciseRepository
{
    public ExerciseRepository(AppDbContext _context) : base(_context)
    {
    }

    public async Task<PagedResult<ExerciseDto>> GetAllExerciseAsync(int page = 1, int pageSize = 20, string? level = null,
        string? category = null)
    {
        IQueryable<Exercise> query = dbSet.AsQueryable();

        if (level != null)
            query = query.Where(e => e.Level == level);
        if (category != null)
            query = query.Where(e => e.Category == category);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<ExerciseDto>()
        {
            Items = items.Select(e => new ExerciseDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Level = e.Level,
                Category = e.Category,
                CategoryDetail = e.CategoryDetail,
                ImageUrl = e.ImageUrl,
                VideoUrl = e.VideoUrl
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}