using FitnessPT_api.Data;
using FitnessPT_api.DataBaseContents.Dtos;
using FitnessPT_api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessPT_api.DataBaseContents.Controllers;

// Controllers/WorkoutRecordsController.cs
using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class WorkoutRecordsController : ControllerBase
    {
        private readonly FitnessDbContext _context;
        private readonly ILogger<WorkoutRecordsController> _logger;

        public WorkoutRecordsController(FitnessDbContext context, ILogger<WorkoutRecordsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 워크아웃 기록 목록 조회
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkoutRecordDto>>> GetWorkoutRecords(
            [FromQuery] int? userId = null,
            [FromQuery] int? exerciseId = null,
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Workoutrecords
                    .Include(w => w.User)
                    .Include(w => w.Exercise)
                    .AsQueryable();

                // 필터링
                if (userId.HasValue)
                    query = query.Where(w => w.UserId == userId);

                if (exerciseId.HasValue)
                    query = query.Where(w => w.ExerciseId == exerciseId);

                if (startDate.HasValue)
                    query = query.Where(w => w.WorkoutDate >= startDate);

                if (endDate.HasValue)
                    query = query.Where(w => w.WorkoutDate <= endDate);

                // 페이징
                var totalCount = await query.CountAsync();
                var records = await query
                    .OrderByDescending(w => w.WorkoutDate)
                    .ThenByDescending(w => w.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(w => new WorkoutRecordDto
                    {
                        RecordId = w.RecordId,
                        UserId = w.UserId,
                        Username = w.User!.Username,
                        ExerciseId = w.ExerciseId,
                        ExerciseName = w.Exercise!.ExerciseName,
                        WorkoutDate = w.WorkoutDate,
                        SetsData = w.SetsData,
                        TotalDurationMinutes = w.TotalDurationMinutes,
                        Notes = w.Notes,
                        CreatedAt = w.CreatedAt
                    })
                    .ToListAsync();

                Response.Headers.Add("X-Total-Count", totalCount.ToString());
                Response.Headers.Add("X-Page", page.ToString());
                Response.Headers.Add("X-Page-Size", pageSize.ToString());

                return Ok(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "워크아웃 기록 목록 조회 중 오류 발생");
                return StatusCode(500, "워크아웃 기록을 불러오는 중 오류가 발생했습니다.");
            }
        }

        /// <summary>
        /// 특정 워크아웃 기록 상세 조회
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkoutRecordDto>> GetWorkoutRecord(int id)
        {
            try
            {
                var record = await _context.Workoutrecords
                    .Include(w => w.User)
                    .Include(w => w.Exercise)
                    .FirstOrDefaultAsync(w => w.RecordId == id);

                if (record == null)
                    return NotFound($"ID {id}에 해당하는 워크아웃 기록을 찾을 수 없습니다.");

                var recordDto = new WorkoutRecordDto
                {
                    RecordId = record.RecordId,
                    UserId = record.UserId,
                    Username = record.User?.Username,
                    ExerciseId = record.ExerciseId,
                    ExerciseName = record.Exercise?.ExerciseName,
                    WorkoutDate = record.WorkoutDate,
                    SetsData = record.SetsData,
                    TotalDurationMinutes = record.TotalDurationMinutes,
                    Notes = record.Notes,
                    CreatedAt = record.CreatedAt
                };

                return Ok(recordDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "워크아웃 기록 상세 조회 중 오류 발생 - ID: {RecordId}", id);
                return StatusCode(500, "워크아웃 기록을 불러오는 중 오류가 발생했습니다.");
            }
        }

        /// <summary>
        /// 새 워크아웃 기록 생성
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<WorkoutRecordDto>> CreateWorkoutRecord(CreateWorkoutRecordDto createDto)
        {
            try
            {
                // 사용자 존재 확인
                var userExists = await _context.Users.AnyAsync(u => u.UserId == createDto.UserId);
                if (!userExists)
                    return BadRequest("존재하지 않는 사용자입니다.");

                // 운동 존재 확인
                var exerciseExists = await _context.Exercises.AnyAsync(e => e.ExerciseId == createDto.ExerciseId);
                if (!exerciseExists)
                    return BadRequest("존재하지 않는 운동입니다.");

                var record = new Workoutrecord
                {
                    UserId = createDto.UserId,
                    ExerciseId = createDto.ExerciseId,
                    WorkoutDate = createDto.WorkoutDate,
                    SetsData = createDto.SetsData,
                    TotalDurationMinutes = createDto.TotalDurationMinutes,
                    Notes = createDto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Workoutrecords.Add(record);
                await _context.SaveChangesAsync();

                // 생성된 기록 정보 반환
                var createdRecord = await _context.Workoutrecords
                    .Include(w => w.User)
                    .Include(w => w.Exercise)
                    .FirstAsync(w => w.RecordId == record.RecordId);

                var recordDto = new WorkoutRecordDto
                {
                    RecordId = createdRecord.RecordId,
                    UserId = createdRecord.UserId,
                    Username = createdRecord.User?.Username,
                    ExerciseId = createdRecord.ExerciseId,
                    ExerciseName = createdRecord.Exercise?.ExerciseName,
                    WorkoutDate = createdRecord.WorkoutDate,
                    SetsData = createdRecord.SetsData,
                    TotalDurationMinutes = createdRecord.TotalDurationMinutes,
                    Notes = createdRecord.Notes,
                    CreatedAt = createdRecord.CreatedAt
                };

                return CreatedAtAction(nameof(GetWorkoutRecord), new { id = record.RecordId }, recordDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "워크아웃 기록 생성 중 오류 발생");
                return StatusCode(500, "워크아웃 기록을 생성하는 중 오류가 발생했습니다.");
            }
        }

        /// <summary>
        /// 워크아웃 기록 수정
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<WorkoutRecordDto>> UpdateWorkoutRecord(int id, UpdateWorkoutRecordDto updateDto)
        {
            try
            {
                var record = await _context.Workoutrecords.FindAsync(id);
                if (record == null)
                    return NotFound($"ID {id}에 해당하는 워크아웃 기록을 찾을 수 없습니다.");

                // 업데이트
                if (updateDto.WorkoutDate.HasValue)
                    record.WorkoutDate = updateDto.WorkoutDate.Value;
                if (!string.IsNullOrEmpty(updateDto.SetsData))
                    record.SetsData = updateDto.SetsData;
                if (updateDto.TotalDurationMinutes.HasValue)
                    record.TotalDurationMinutes = updateDto.TotalDurationMinutes;
                if (updateDto.Notes != null)
                    record.Notes = updateDto.Notes;

                await _context.SaveChangesAsync();

                // 업데이트된 정보 반환
                var updatedRecord = await _context.Workoutrecords
                    .Include(w => w.User)
                    .Include(w => w.Exercise)
                    .FirstAsync(w => w.RecordId == id);

                var recordDto = new WorkoutRecordDto
                {
                    RecordId = updatedRecord.RecordId,
                    UserId = updatedRecord.UserId,
                    Username = updatedRecord.User?.Username,
                    ExerciseId = updatedRecord.ExerciseId,
                    ExerciseName = updatedRecord.Exercise?.ExerciseName,
                    WorkoutDate = updatedRecord.WorkoutDate,
                    SetsData = updatedRecord.SetsData,
                    TotalDurationMinutes = updatedRecord.TotalDurationMinutes,
                    Notes = updatedRecord.Notes,
                    CreatedAt = updatedRecord.CreatedAt
                };

                return Ok(recordDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "워크아웃 기록 수정 중 오류 발생 - ID: {RecordId}", id);
                return StatusCode(500, "워크아웃 기록을 수정하는 중 오류가 발생했습니다.");
            }
        }

        /// <summary>
        /// 워크아웃 기록 삭제
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkoutRecord(int id)
        {
            try
            {
                var record = await _context.Workoutrecords.FindAsync(id);
                if (record == null)
                    return NotFound($"ID {id}에 해당하는 워크아웃 기록을 찾을 수 없습니다.");

                _context.Workoutrecords.Remove(record);
                await _context.SaveChangesAsync();

                return Ok(new { message = "워크아웃 기록이 삭제되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "워크아웃 기록 삭제 중 오류 발생 - ID: {RecordId}", id);
                return StatusCode(500, "워크아웃 기록을 삭제하는 중 오류가 발생했습니다.");
            }
        }

        /// <summary>
        /// 사용자별 워크아웃 기록 조회
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<WorkoutRecordDto>>> GetUserWorkoutRecords(
            int userId,
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Workoutrecords
                    .Include(w => w.Exercise)
                    .Where(w => w.UserId == userId)
                    .AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(w => w.WorkoutDate >= startDate);

                if (endDate.HasValue)
                    query = query.Where(w => w.WorkoutDate <= endDate);

                var totalCount = await query.CountAsync();
                var records = await query
                    .OrderByDescending(w => w.WorkoutDate)
                    .ThenByDescending(w => w.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(w => new WorkoutRecordDto
                    {
                        RecordId = w.RecordId,
                        UserId = w.UserId,
                        ExerciseId = w.ExerciseId,
                        ExerciseName = w.Exercise!.ExerciseName,
                        WorkoutDate = w.WorkoutDate,
                        SetsData = w.SetsData,
                        TotalDurationMinutes = w.TotalDurationMinutes,
                        Notes = w.Notes,
                        CreatedAt = w.CreatedAt
                    })
                    .ToListAsync();

                Response.Headers.Add("X-Total-Count", totalCount.ToString());
                Response.Headers.Add("X-Page", page.ToString());
                Response.Headers.Add("X-Page-Size", pageSize.ToString());

                return Ok(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "사용자 워크아웃 기록 조회 중 오류 발생 - UserId: {UserId}", userId);
                return StatusCode(500, "사용자 워크아웃 기록을 불러오는 중 오류가 발생했습니다.");
            }
        }

        /// <summary>
        /// 운동별 워크아웃 기록 조회
        /// </summary>
        [HttpGet("exercise/{exerciseId}")]
        public async Task<ActionResult<IEnumerable<WorkoutRecordDto>>> GetExerciseWorkoutRecords(
            int exerciseId,
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Workoutrecords
                    .Include(w => w.User)
                    .Where(w => w.ExerciseId == exerciseId)
                    .AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(w => w.WorkoutDate >= startDate);

                if (endDate.HasValue)
                    query = query.Where(w => w.WorkoutDate <= endDate);

                var totalCount = await query.CountAsync();
                var records = await query
                    .OrderByDescending(w => w.WorkoutDate)
                    .ThenByDescending(w => w.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(w => new WorkoutRecordDto
                    {
                        RecordId = w.RecordId,
                        UserId = w.UserId,
                        Username = w.User!.Username,
                        ExerciseId = w.ExerciseId,
                        WorkoutDate = w.WorkoutDate,
                        SetsData = w.SetsData,
                        TotalDurationMinutes = w.TotalDurationMinutes,
                        Notes = w.Notes,
                        CreatedAt = w.CreatedAt
                    })
                    .ToListAsync();

                Response.Headers.Add("X-Total-Count", totalCount.ToString());
                Response.Headers.Add("X-Page", page.ToString());
                Response.Headers.Add("X-Page-Size", pageSize.ToString());

                return Ok(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "운동별 워크아웃 기록 조회 중 오류 발생 - ExerciseId: {ExerciseId}", exerciseId);
                return StatusCode(500, "운동별 워크아웃 기록을 불러오는 중 오류가 발생했습니다.");
            }
        }

        /// <summary>
        /// 워크아웃 통계 조회
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult> GetWorkoutStatistics([FromQuery] int? userId = null)
        {
            try
            {
                var query = _context.Workoutrecords.AsQueryable();
                
                if (userId.HasValue)
                    query = query.Where(w => w.UserId == userId);

                var totalWorkouts = await query.CountAsync();
                var totalDuration = await query.SumAsync(w => w.TotalDurationMinutes ?? 0);
                var uniqueExercises = await query.Select(w => w.ExerciseId).Distinct().CountAsync();
                var uniqueUsers = userId.HasValue ? 1 : await query.Select(w => w.UserId).Distinct().CountAsync();

                var recentWorkouts = await query
                    .Where(w => w.WorkoutDate >= DateOnly.FromDateTime(DateTime.Now.AddDays(-30)))
                    .CountAsync();

                var popularExercises = await query
                    .Include(w => w.Exercise)
                    .GroupBy(w => new { w.ExerciseId, w.Exercise!.ExerciseName })
                    .Select(g => new
                    {
                        ExerciseId = g.Key.ExerciseId,
                        ExerciseName = g.Key.ExerciseName,
                        WorkoutCount = g.Count(),
                        TotalDuration = g.Sum(w => w.TotalDurationMinutes ?? 0)
                    })
                    .OrderByDescending(e => e.WorkoutCount)
                    .Take(10)
                    .ToListAsync();

                var monthlyStats = await query
                    .GroupBy(w => new { w.WorkoutDate.Year, w.WorkoutDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        WorkoutCount = g.Count(),
                        TotalDuration = g.Sum(w => w.TotalDurationMinutes ?? 0)
                    })
                    .OrderByDescending(s => s.Year)
                    .ThenByDescending(s => s.Month)
                    .Take(12)
                    .ToListAsync();

                var statistics = new
                {
                    TotalWorkouts = totalWorkouts,
                    TotalDurationMinutes = totalDuration,
                    UniqueExercises = uniqueExercises,
                    UniqueUsers = uniqueUsers,
                    RecentWorkouts = recentWorkouts,
                    AverageDurationPerWorkout = totalWorkouts > 0 ? (double)totalDuration / totalWorkouts : 0,
                    PopularExercises = popularExercises,
                    MonthlyStats = monthlyStats
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "워크아웃 통계 조회 중 오류 발생");
                return StatusCode(500, "워크아웃 통계를 불러오는 중 오류가 발생했습니다.");
            }
        }

        /// <summary>
        /// 일별 워크아웃 캘린더 조회
        /// </summary>
        [HttpGet("calendar")]
        public async Task<ActionResult> GetWorkoutCalendar(
            [FromQuery] int userId,
            [FromQuery] int year = 0,
            [FromQuery] int month = 0)
        {
            try
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;

                var startDate = new DateOnly(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var workouts = await _context.Workoutrecords
                    .Include(w => w.Exercise)
                    .Where(w => w.UserId == userId && w.WorkoutDate >= startDate && w.WorkoutDate <= endDate)
                    .GroupBy(w => w.WorkoutDate)
                    .Select(g => new
                    {
                        Date = g.Key,
                        WorkoutCount = g.Count(),
                        TotalDuration = g.Sum(w => w.TotalDurationMinutes ?? 0),
                        Exercises = g.Select(w => new
                        {
                            ExerciseId = w.ExerciseId,
                            ExerciseName = w.Exercise!.ExerciseName,
                            Duration = w.TotalDurationMinutes
                        }).ToList()
                    })
                    .OrderBy(d => d.Date)
                    .ToListAsync();

                return Ok(workouts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "워크아웃 캘린더 조회 중 오류 발생");
                return StatusCode(500, "워크아웃 캘린더를 불러오는 중 오류가 발생했습니다.");
            }
        }

        /// <summary>
        /// 벌크 워크아웃 기록 생성
        /// </summary>
        [HttpPost("bulk")]
        public async Task<ActionResult> CreateBulkWorkoutRecords([FromBody] List<CreateWorkoutRecordDto> recordDtos)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var records = new List<Workoutrecord>();
                foreach (var dto in recordDtos)
                {
                    // 사용자 및 운동 존재 확인은 별도로 수행하거나 제외
                    var record = new Workoutrecord
                    {
                        UserId = dto.UserId,
                        ExerciseId = dto.ExerciseId,
                        WorkoutDate = dto.WorkoutDate,
                        SetsData = dto.SetsData,
                        TotalDurationMinutes = dto.TotalDurationMinutes,
                        Notes = dto.Notes,
                        CreatedAt = DateTime.UtcNow
                    };
                    records.Add(record);
                }

                _context.Workoutrecords.AddRange(records);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = $"{records.Count}개의 워크아웃 기록이 생성되었습니다.", count = records.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "벌크 워크아웃 기록 생성 중 오류 발생");
                return StatusCode(500, "워크아웃 기록들을 생성하는 중 오류가 발생했습니다.");
            }
        }
    }