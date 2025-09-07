using FitnessPT_api.Data;
using FitnessPT_api.DataBaseContents.Dtos;
using FitnessPT_api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessPT_api.DataBaseContents.Controllers;

// Controllers/BodyRecordsController.cs
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BodyRecordsController : ControllerBase
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<BodyRecordsController> _logger;

    public BodyRecordsController(FitnessDbContext context, ILogger<BodyRecordsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 신체 기록 목록 조회
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BodyRecordDto>>> GetBodyRecords(
        [FromQuery] int? userId = null,
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _context.Bodyrecords
                .Include(b => b.User)
                .AsQueryable();

            // 필터링
            if (userId.HasValue)
                query = query.Where(b => b.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(b => b.RecordedDate >= startDate);

            if (endDate.HasValue)
                query = query.Where(b => b.RecordedDate <= endDate);

            // 페이징
            var totalCount = await query.CountAsync();
            var records = await query
                .OrderByDescending(b => b.RecordedDate)
                .ThenByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BodyRecordDto
                {
                    RecordId = b.RecordId,
                    UserId = b.UserId,
                    Username = b.User!.Username,
                    RecordedDate = b.RecordedDate,
                    WeightKg = b.WeightKg,
                    BodyFatPercentage = b.BodyFatPercentage,
                    MuscleMassKg = b.MuscleMassKg,
                    Notes = b.Notes,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Page", page.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "신체 기록 목록 조회 중 오류 발생");
            return StatusCode(500, "신체 기록을 불러오는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 특정 신체 기록 상세 조회
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BodyRecordDto>> GetBodyRecord(int id)
    {
        try
        {
            var record = await _context.Bodyrecords
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.RecordId == id);

            if (record == null)
                return NotFound($"ID {id}에 해당하는 신체 기록을 찾을 수 없습니다.");

            var recordDto = new BodyRecordDto
            {
                RecordId = record.RecordId,
                UserId = record.UserId,
                Username = record.User?.Username,
                RecordedDate = record.RecordedDate,
                WeightKg = record.WeightKg,
                BodyFatPercentage = record.BodyFatPercentage,
                MuscleMassKg = record.MuscleMassKg,
                Notes = record.Notes,
                CreatedAt = record.CreatedAt
            };

            return Ok(recordDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "신체 기록 상세 조회 중 오류 발생 - ID: {RecordId}", id);
            return StatusCode(500, "신체 기록을 불러오는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 새 신체 기록 생성
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BodyRecordDto>> CreateBodyRecord(CreateBodyRecordDto createDto)
    {
        try
        {
            // 사용자 존재 확인
            var userExists = await _context.Users.AnyAsync(u => u.UserId == createDto.UserId);
            if (!userExists)
                return BadRequest("존재하지 않는 사용자입니다.");

            var record = new Bodyrecord
            {
                UserId = createDto.UserId,
                RecordedDate = createDto.RecordedDate,
                WeightKg = createDto.WeightKg,
                BodyFatPercentage = createDto.BodyFatPercentage,
                MuscleMassKg = createDto.MuscleMassKg,
                Notes = createDto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bodyrecords.Add(record);
            await _context.SaveChangesAsync();

            // 생성된 기록 정보 반환
            var createdRecord = await _context.Bodyrecords
                .Include(b => b.User)
                .FirstAsync(b => b.RecordId == record.RecordId);

            var recordDto = new BodyRecordDto
            {
                RecordId = createdRecord.RecordId,
                UserId = createdRecord.UserId,
                Username = createdRecord.User?.Username,
                RecordedDate = createdRecord.RecordedDate,
                WeightKg = createdRecord.WeightKg,
                BodyFatPercentage = createdRecord.BodyFatPercentage,
                MuscleMassKg = createdRecord.MuscleMassKg,
                Notes = createdRecord.Notes,
                CreatedAt = createdRecord.CreatedAt
            };

            return CreatedAtAction(nameof(GetBodyRecord), new { id = record.RecordId }, recordDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "신체 기록 생성 중 오류 발생");
            return StatusCode(500, "신체 기록을 생성하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 신체 기록 수정
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<BodyRecordDto>> UpdateBodyRecord(int id, UpdateBodyRecordDto updateDto)
    {
        try
        {
            var record = await _context.Bodyrecords.FindAsync(id);
            if (record == null)
                return NotFound($"ID {id}에 해당하는 신체 기록을 찾을 수 없습니다.");

            // 업데이트
            if (updateDto.RecordedDate.HasValue)
                record.RecordedDate = updateDto.RecordedDate.Value;
            if (updateDto.WeightKg.HasValue)
                record.WeightKg = updateDto.WeightKg;
            if (updateDto.BodyFatPercentage.HasValue)
                record.BodyFatPercentage = updateDto.BodyFatPercentage;
            if (updateDto.MuscleMassKg.HasValue)
                record.MuscleMassKg = updateDto.MuscleMassKg;
            if (updateDto.Notes != null)
                record.Notes = updateDto.Notes;

            await _context.SaveChangesAsync();

            // 업데이트된 정보 반환
            var updatedRecord = await _context.Bodyrecords
                .Include(b => b.User)
                .FirstAsync(b => b.RecordId == id);

            var recordDto = new BodyRecordDto
            {
                RecordId = updatedRecord.RecordId,
                UserId = updatedRecord.UserId,
                Username = updatedRecord.User?.Username,
                RecordedDate = updatedRecord.RecordedDate,
                WeightKg = updatedRecord.WeightKg,
                BodyFatPercentage = updatedRecord.BodyFatPercentage,
                MuscleMassKg = updatedRecord.MuscleMassKg,
                Notes = updatedRecord.Notes,
                CreatedAt = updatedRecord.CreatedAt
            };

            return Ok(recordDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "신체 기록 수정 중 오류 발생 - ID: {RecordId}", id);
            return StatusCode(500, "신체 기록을 수정하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 신체 기록 삭제
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBodyRecord(int id)
    {
        try
        {
            var record = await _context.Bodyrecords.FindAsync(id);
            if (record == null)
                return NotFound($"ID {id}에 해당하는 신체 기록을 찾을 수 없습니다.");

            _context.Bodyrecords.Remove(record);
            await _context.SaveChangesAsync();

            return Ok(new { message = "신체 기록이 삭제되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "신체 기록 삭제 중 오류 발생 - ID: {RecordId}", id);
            return StatusCode(500, "신체 기록을 삭제하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 사용자별 신체 기록 조회
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<BodyRecordDto>>> GetUserBodyRecords(
        int userId,
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _context.Bodyrecords
                .Where(b => b.UserId == userId)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(b => b.RecordedDate >= startDate);

            if (endDate.HasValue)
                query = query.Where(b => b.RecordedDate <= endDate);

            var totalCount = await query.CountAsync();
            var records = await query
                .OrderByDescending(b => b.RecordedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BodyRecordDto
                {
                    RecordId = b.RecordId,
                    UserId = b.UserId,
                    RecordedDate = b.RecordedDate,
                    WeightKg = b.WeightKg,
                    BodyFatPercentage = b.BodyFatPercentage,
                    MuscleMassKg = b.MuscleMassKg,
                    Notes = b.Notes,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Page", page.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "사용자 신체 기록 조회 중 오류 발생 - UserId: {UserId}", userId);
            return StatusCode(500, "사용자 신체 기록을 불러오는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 사용자의 최신 신체 기록 조회
    /// </summary>
    [HttpGet("user/{userId}/latest")]
    public async Task<ActionResult<BodyRecordDto>> GetLatestBodyRecord(int userId)
    {
        try
        {
            var record = await _context.Bodyrecords
                .Include(b => b.User)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.RecordedDate)
                .ThenByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();

            if (record == null)
                return NotFound($"사용자 ID {userId}의 신체 기록을 찾을 수 없습니다.");

            var recordDto = new BodyRecordDto
            {
                RecordId = record.RecordId,
                UserId = record.UserId,
                Username = record.User?.Username,
                RecordedDate = record.RecordedDate,
                WeightKg = record.WeightKg,
                BodyFatPercentage = record.BodyFatPercentage,
                MuscleMassKg = record.MuscleMassKg,
                Notes = record.Notes,
                CreatedAt = record.CreatedAt
            };

            return Ok(recordDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "최신 신체 기록 조회 중 오류 발생 - UserId: {UserId}", userId);
            return StatusCode(500, "최신 신체 기록을 불러오는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 신체 기록 통계 조회
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult> GetBodyRecordStatistics([FromQuery] int? userId = null)
    {
        try
        {
            var query = _context.Bodyrecords.AsQueryable();

            if (userId.HasValue)
                query = query.Where(b => b.UserId == userId);

            var totalRecords = await query.CountAsync();
            var uniqueUsers = userId.HasValue ? 1 : await query.Select(b => b.UserId).Distinct().CountAsync();

            var weightStats = await query
                .Where(b => b.WeightKg.HasValue)
                .Select(b => b.WeightKg!.Value)
                .ToListAsync();

            var bodyFatStats = await query
                .Where(b => b.BodyFatPercentage.HasValue)
                .Select(b => b.BodyFatPercentage!.Value)
                .ToListAsync();

            var muscleMassStats = await query
                .Where(b => b.MuscleMassKg.HasValue)
                .Select(b => b.MuscleMassKg!.Value)
                .ToListAsync();

            var recentRecords = await query
                .Where(b => b.RecordedDate >= DateOnly.FromDateTime(DateTime.Now.AddDays(-30)))
                .CountAsync();

            var statistics = new
            {
                TotalRecords = totalRecords,
                UniqueUsers = uniqueUsers,
                RecentRecords = recentRecords,
                WeightStatistics = new
                {
                    Count = weightStats.Count,
                    Average = weightStats.Any() ? weightStats.Average() : 0,
                    Min = weightStats.Any() ? weightStats.Min() : 0,
                    Max = weightStats.Any() ? weightStats.Max() : 0
                },
                BodyFatStatistics = new
                {
                    Count = bodyFatStats.Count,
                    Average = bodyFatStats.Any() ? bodyFatStats.Average() : 0,
                    Min = bodyFatStats.Any() ? bodyFatStats.Min() : 0,
                    Max = bodyFatStats.Any() ? bodyFatStats.Max() : 0
                },
                MuscleMassStatistics = new
                {
                    Count = muscleMassStats.Count,
                    Average = muscleMassStats.Any() ? muscleMassStats.Average() : 0,
                    Min = muscleMassStats.Any() ? muscleMassStats.Min() : 0,
                    Max = muscleMassStats.Any() ? muscleMassStats.Max() : 0
                }
            };

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "신체 기록 통계 조회 중 오류 발생");
            return StatusCode(500, "신체 기록 통계를 불러오는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 사용자의 체중 변화 추이 조회
    /// </summary>
    [HttpGet("user/{userId}/weight-trend")]
    public async Task<ActionResult> GetWeightTrend(
        int userId,
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null)
    {
        try
        {
            var query = _context.Bodyrecords
                .Where(b => b.UserId == userId && b.WeightKg.HasValue)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(b => b.RecordedDate >= startDate);

            if (endDate.HasValue)
                query = query.Where(b => b.RecordedDate <= endDate);

            var weightData = await query
                .OrderBy(b => b.RecordedDate)
                .Select(b => new
                {
                    Date = b.RecordedDate,
                    Weight = b.WeightKg!.Value,
                    BodyFat = b.BodyFatPercentage,
                    MuscleMass = b.MuscleMassKg
                })
                .ToListAsync();

            if (!weightData.Any())
                return Ok(new { message = "체중 기록이 없습니다.", data = new List<object>() });

            var firstWeight = weightData.First().Weight;
            var lastWeight = weightData.Last().Weight;
            var totalChange = lastWeight - firstWeight;
            var changePercentage = firstWeight != 0 ? (totalChange / firstWeight) * 100 : 0;

            var result = new
            {
                UserId = userId,
                StartDate = weightData.First().Date,
                EndDate = weightData.Last().Date,
                StartWeight = firstWeight,
                EndWeight = lastWeight,
                TotalChange = totalChange,
                ChangePercentage = Math.Round(changePercentage, 2),
                RecordCount = weightData.Count,
                Data = weightData
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "체중 변화 추이 조회 중 오류 발생 - UserId: {UserId}", userId);
            return StatusCode(500, "체중 변화 추이를 불러오는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 체성분 분석 결과 조회
    /// </summary>
    [HttpGet("user/{userId}/body-composition")]
    public async Task<ActionResult> GetBodyComposition(int userId)
    {
        try
        {
            var latestRecord = await _context.Bodyrecords
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.RecordedDate)
                .ThenByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestRecord == null)
                return NotFound($"사용자 ID {userId}의 신체 기록을 찾을 수 없습니다.");

            // 사용자 프로필에서 키 정보 가져오기
            var userProfile = await _context.Userprofiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var result = new
            {
                UserId = userId,
                RecordedDate = latestRecord.RecordedDate,
                Weight = latestRecord.WeightKg,
                BodyFatPercentage = latestRecord.BodyFatPercentage,
                MuscleMass = latestRecord.MuscleMassKg,
                Height = userProfile?.HeightCm,
                BMI = !string.IsNullOrEmpty(userProfile?.HeightCm?.ToString()) && latestRecord.WeightKg.HasValue
                    ? Math.Round(
                        (decimal)(latestRecord.WeightKg.Value /
                                  (decimal)Math.Pow((double)(userProfile.HeightCm.Value / 100), 2)), 2)
                    : (decimal?)null,
                FatMass = latestRecord.WeightKg.HasValue && latestRecord.BodyFatPercentage.HasValue
                    ? Math.Round(latestRecord.WeightKg.Value * latestRecord.BodyFatPercentage.Value / 100, 2)
                    : (decimal?)null,
                LeanBodyMass = latestRecord.WeightKg.HasValue && latestRecord.BodyFatPercentage.HasValue
                    ? Math.Round(latestRecord.WeightKg.Value * (1 - latestRecord.BodyFatPercentage.Value / 100), 2)
                    : (decimal?)null
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "체성분 분석 조회 중 오류 발생 - UserId: {UserId}", userId);
            return StatusCode(500, "체성분 분석을 불러오는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 벌크 신체 기록 생성
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult> CreateBulkBodyRecords([FromBody] List<CreateBodyRecordDto> recordDtos)
    {
        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var records = new List<Bodyrecord>();
            foreach (var dto in recordDtos)
            {
                var record = new Bodyrecord
                {
                    UserId = dto.UserId,
                    RecordedDate = dto.RecordedDate,
                    WeightKg = dto.WeightKg,
                    BodyFatPercentage = dto.BodyFatPercentage,
                    MuscleMassKg = dto.MuscleMassKg,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow
                };
                records.Add(record);
            }

            _context.Bodyrecords.AddRange(records);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = $"{records.Count}개의 신체 기록이 생성되었습니다.", count = records.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "벌크 신체 기록 생성 중 오류 발생");
            return StatusCode(500, "신체 기록들을 생성하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 월별 평균 체중 조회
    /// </summary>
    [HttpGet("user/{userId}/monthly-averages")]
    public async Task<ActionResult> GetMonthlyAverages(int userId, [FromQuery] int months = 12)
    {
        try
        {
            var startDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-months));

            var monthlyData = await _context.Bodyrecords
                .Where(b => b.UserId == userId && b.RecordedDate >= startDate)
                .GroupBy(b => new { b.RecordedDate.Year, b.RecordedDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    AvgWeight = g.Where(b => b.WeightKg.HasValue).Average(b => b.WeightKg),
                    AvgBodyFat = g.Where(b => b.BodyFatPercentage.HasValue).Average(b => b.BodyFatPercentage),
                    AvgMuscleMass = g.Where(b => b.MuscleMassKg.HasValue).Average(b => b.MuscleMassKg),
                    RecordCount = g.Count()
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToListAsync();

            return Ok(monthlyData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "월별 평균 조회 중 오류 발생 - UserId: {UserId}", userId);
            return StatusCode(500, "월별 평균을 불러오는 중 오류가 발생했습니다.");
        }
    }
}