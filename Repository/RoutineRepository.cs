using FitnessPT_api.Data;
using FitnessPT_api.Dtos;
using FitnessPT_api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessPT_api.Repository;

public interface IRoutineRepository : IRepository<Routine>
{
    Task<Routine?> GetWithExercisesAsync(int id);
    Task<PagedResult<Routine>> GetAllRoutinesAsync(int page = 1, int pageSize = 20, int userid = 0, string? level = null, string? category = null);
    Task<List<RoutineExercise>> GetRoutineDetail(int routineId);
    Task<bool> AddRoutineExercise(int routineId, List<RoutineExerciseDto> entities);
    Task<bool> AddRoutineExercise(RoutineExerciseDto entity);
    Task<RoutineExercise> UpdateRoutineExerciseById(int exerciseId, RoutineExerciseDto entity);
    Task<RoutineExercise> DeleteRoutineExerciseById(int id);
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

    public async Task<PagedResult<Routine>> GetAllRoutinesAsync(int page = 1, int pageSize = 20, int userid = 0, string? level = null,
        string? category = null)
    {
        IQueryable<Routine> query = dbSet.Where(r => r.CreatedUser == userid);

        if (level != null)
            query = query.Where(r => r.Level == level);
        if (category != null)
            query = query.Where(r => r.Category == category);
        
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

    public async Task<List<RoutineExercise>> GetRoutineDetail(int routineId)
    {
        var items = await context.RoutineExercises.Where(r => r.RoutineId == routineId)
            .AsNoTracking()
            .ToListAsync();

        return items;
    }

    public async Task<bool> AddRoutineExercise(int routineId, List<RoutineExerciseDto> entities)
    {
        foreach (RoutineExerciseDto entity in entities)
        {
            try
            {
                var routineExercise = new RoutineExercise
                {
                    RoutineId = routineId,
                    ExerciseId = entity.ExerciseId,
                    OrderIndex = entity.OrderIndex,
                    Sets = entity.Sets,
                    Reps = entity.Reps,
                    DurationSeconds = entity.DurationSeconds,
                    RestSeconds = entity.RestSeconds
                };

                await context.RoutineExercises.AddAsync(routineExercise);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("루틴 운동 세부할목 추가 오류");
                return false;
            }
        }

        return true;
    }
    
    public async Task<bool> AddRoutineExercise(RoutineExerciseDto entity)
    {
        try
        {
            var routineExercise = new RoutineExercise
            {
                RoutineId = entity.RoutineId,
                ExerciseId = entity.ExerciseId,
                OrderIndex = entity.OrderIndex,
                Sets = entity.Sets,
                Reps = entity.Reps,
                DurationSeconds = entity.DurationSeconds,
                RestSeconds = entity.RestSeconds
            };

            await context.RoutineExercises.AddAsync(routineExercise);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"루틴 추가중 오류 발생 : {ex}");
            return false;
        }
    }
    
    public async Task<RoutineExercise> UpdateRoutineExerciseById(int id, RoutineExerciseDto entity)
    {
        var exercise = await context.RoutineExercises.FindAsync(id);

        if (exercise == null || exercise.RoutineId != entity.RoutineId)
            return null;
        
        exercise.ExerciseId = entity.ExerciseId;
        exercise.OrderIndex = entity.OrderIndex;
        exercise.Sets = entity.Sets;
        exercise.Reps = entity.Reps;
        exercise.DurationSeconds = entity.DurationSeconds;
        exercise.RestSeconds = entity.RestSeconds;

        context.RoutineExercises.Update(exercise);
        context.SaveChangesAsync();

        return exercise;
    }

    public async Task<RoutineExercise> DeleteRoutineExerciseById(int id)
    {
        var exercise = await context.RoutineExercises.FindAsync(id);

        context.RoutineExercises.Remove(exercise);
        context.SaveChangesAsync();

        return exercise;
    }

    public async Task<List<Routine>> GetAllRoutinesAsync()
    {
        return await dbSet.Where(r=>r.CreatedUser == null).ToListAsync();
    }
}