using FitnessPT_api.Data;
using FitnessPT_api.Dtos;
using FitnessPT_api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessPT_api.Repository;

public interface IRoutineRepository : IRepository<Routine>
{
    Task<Routine?> GetWithExercisesAsync(int id);
    Task<PagedResult<Routine>> GetAllRoutinesAsync(int page = 1, int pageSize = 20, int userid = 0);
}

public class RoutineRepository : Repository<Routine>, IRoutineRepository
{
    public RoutineRepository(AppDbContext _context) : base(_context) { }

    public async Task<Routine?> GetWithExercisesAsync(int id)
    {
        return await dbSet.Include(r => r.RoutineExercises)
            .ThenInclude(re => re.Exercise)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<PagedResult<Routine>> GetAllRoutinesAsync(int page = 1, int pageSize = 20, int userid = 0)
    {
        IQueryable<Routine> query = dbSet.Where(r => r.CreatedUser == userid);

        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<Routine>()
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<List<Routine>> GetAllRoutinesAsync()
    {
        return await dbSet.Where(r=>r.CreatedUser == null).ToListAsync();
    }
}