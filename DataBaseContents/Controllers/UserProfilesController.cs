using FitnessPT_api.Data;
using FitnessPT_api.DataBaseContents.Dtos;
using FitnessPT_api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessPT_api.DataBaseContents.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserProfilesController : ControllerBase
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<UserProfilesController> _logger;

    public UserProfilesController(FitnessDbContext context, ILogger<UserProfilesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 특정 사용자의 프로필 조회
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile(int userId)
    {
        try
        {
            var profile = await _context.Userprofiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return NotFound($"사용자 ID {userId}에 해당하는 프로필을 찾을 수 없습니다.");

            var profileDto = new UserProfileDto
            {
                UserId = profile.UserId,
                Gender = profile.Gender,
                BirthDate = profile.BirthDate,
                HeightCm = profile.HeightCm,
                CurrentWeightKg = profile.CurrentWeightKg,
                FitnessLevel = profile.FitnessLevel,
                FitnessGoal = profile.FitnessGoal,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };

            return Ok(profileDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "사용자 프로필 조회 중 오류 발생 - UserId: {UserId}", userId);
            return StatusCode(500, "사용자 프로필을 불러오는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 사용자 프로필 생성
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserProfileDto>> CreateUserProfile(CreateUserProfileDto createDto)
    {
        try
        {
            // 사용자 존재 확인
            var userExists = await _context.Users.AnyAsync(u => u.UserId == createDto.UserId);
            if (!userExists)
                return BadRequest("존재하지 않는 사용자입니다.");

            // 프로필 중복 확인
            var profileExists = await _context.Userprofiles.AnyAsync(p => p.UserId == createDto.UserId);
            if (profileExists)
                return BadRequest("이미 프로필이 존재하는 사용자입니다.");

            var profile = new Userprofile
            {
                UserId = createDto.UserId,
                Gender = createDto.Gender,
                BirthDate = createDto.BirthDate,
                HeightCm = createDto.HeightCm,
                CurrentWeightKg = createDto.CurrentWeightKg,
                FitnessLevel = createDto.FitnessLevel,
                FitnessGoal = createDto.FitnessGoal,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Userprofiles.Add(profile);
            await _context.SaveChangesAsync();

            var profileDto = new UserProfileDto
            {
                UserId = profile.UserId,
                Gender = profile.Gender,
                BirthDate = profile.BirthDate,
                HeightCm = profile.HeightCm,
                CurrentWeightKg = profile.CurrentWeightKg,
                FitnessLevel = profile.FitnessLevel,
                FitnessGoal = profile.FitnessGoal,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };

            return CreatedAtAction(nameof(GetUserProfile), new { userId = profile.UserId }, profileDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "사용자 프로필 생성 중 오류 발생");
            return StatusCode(500, "사용자 프로필을 생성하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 사용자 프로필 수정
    /// </summary>
    [HttpPut("{userId}")]
    public async Task<ActionResult<UserProfileDto>> UpdateUserProfile(int userId, UpdateUserProfileDto updateDto)
    {
        try
        {
            var profile = await _context.Userprofiles.FindAsync(userId);
            if (profile == null)
                return NotFound($"사용자 ID {userId}에 해당하는 프로필을 찾을 수 없습니다.");

            // 업데이트
            if (!string.IsNullOrEmpty(updateDto.Gender))
                profile.Gender = updateDto.Gender;
            if (updateDto.BirthDate.HasValue)
                profile.BirthDate = updateDto.BirthDate;
            if (updateDto.HeightCm.HasValue)
                profile.HeightCm = updateDto.HeightCm;
            if (updateDto.CurrentWeightKg.HasValue)
                profile.CurrentWeightKg = updateDto.CurrentWeightKg;
            if (!string.IsNullOrEmpty(updateDto.FitnessLevel))
                profile.FitnessLevel = updateDto.FitnessLevel;
            if (!string.IsNullOrEmpty(updateDto.FitnessGoal))
                profile.FitnessGoal = updateDto.FitnessGoal;

            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var profileDto = new UserProfileDto
            {
                UserId = profile.UserId,
                Gender = profile.Gender,
                BirthDate = profile.BirthDate,
                HeightCm = profile.HeightCm,
                CurrentWeightKg = profile.CurrentWeightKg,
                FitnessLevel = profile.FitnessLevel,
                FitnessGoal = profile.FitnessGoal,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };

            return Ok(profileDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "사용자 프로필 수정 중 오류 발생 - UserId: {UserId}", userId);
            return StatusCode(500, "사용자 프로필을 수정하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 사용자 프로필 삭제
    /// </summary>
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUserProfile(int userId)
    {
        try
        {
            var profile = await _context.Userprofiles.FindAsync(userId);
            if (profile == null)
                return NotFound($"사용자 ID {userId}에 해당하는 프로필을 찾을 수 없습니다.");

            _context.Userprofiles.Remove(profile);
            await _context.SaveChangesAsync();

            return Ok(new { message = "사용자 프로필이 삭제되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "사용자 프로필 삭제 중 오류 발생 - UserId: {UserId}", userId);
            return StatusCode(500, "사용자 프로필을 삭제하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 프로필 생성 또는 업데이트 (Upsert)
    /// </summary>
    [HttpPut("{userId}/upsert")]
    public async Task<ActionResult<UserProfileDto>> UpsertUserProfile(int userId, UpdateUserProfileDto updateDto)
    {
        try
        {
            // 사용자 존재 확인
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
                return BadRequest("존재하지 않는 사용자입니다.");

            var profile = await _context.Userprofiles.FindAsync(userId);

            if (profile == null)
            {
                // 프로필이 없으면 새로 생성
                profile = new Userprofile
                {
                    UserId = userId,
                    Gender = updateDto.Gender,
                    BirthDate = updateDto.BirthDate,
                    HeightCm = updateDto.HeightCm,
                    CurrentWeightKg = updateDto.CurrentWeightKg,
                    FitnessLevel = updateDto.FitnessLevel,
                    FitnessGoal = updateDto.FitnessGoal,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Userprofiles.Add(profile);
            }
            else
            {
                // 프로필이 있으면 업데이트
                if (!string.IsNullOrEmpty(updateDto.Gender))
                    profile.Gender = updateDto.Gender;
                if (updateDto.BirthDate.HasValue)
                    profile.BirthDate = updateDto.BirthDate;
                if (updateDto.HeightCm.HasValue)
                    profile.HeightCm = updateDto.HeightCm;
                if (updateDto.CurrentWeightKg.HasValue)
                    profile.CurrentWeightKg = updateDto.CurrentWeightKg;
                if (!string.IsNullOrEmpty(updateDto.FitnessLevel))
                    profile.FitnessLevel = updateDto.FitnessLevel;
                if (!string.IsNullOrEmpty(updateDto.FitnessGoal))
                    profile.FitnessGoal = updateDto.FitnessGoal;

                profile.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var profileDto = new UserProfileDto
            {
                UserId = profile.UserId,
                Gender = profile.Gender,
                BirthDate = profile.BirthDate,
                HeightCm = profile.HeightCm,
                CurrentWeightKg = profile.CurrentWeightKg,
                FitnessLevel = profile.FitnessLevel,
                FitnessGoal = profile.FitnessGoal,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };

            return Ok(profileDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "사용자 프로필 Upsert 중 오류 발생 - UserId: {UserId}", userId);
            return StatusCode(500, "사용자 프로필을 저장하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// BMI 계산
    /// </summary>
    [HttpGet("{userId}/bmi")]
    public async Task<ActionResult> CalculateBMI(int userId)
    {
        try
        {
            var profile = await _context.Userprofiles.FindAsync(userId);
            if (profile == null)
                return NotFound($"사용자 ID {userId}에 해당하는 프로필을 찾을 수 없습니다.");

            if (!profile.HeightCm.HasValue || !profile.CurrentWeightKg.HasValue)
                return BadRequest("키와 몸무게 정보가 필요합니다.");

            var heightM = profile.HeightCm.Value / 100m;
            var bmi = profile.CurrentWeightKg.Value / (heightM * heightM);

            string category;
            if (bmi < 18.5m)
                category = "저체중";
            else if (bmi < 23m)
                category = "정상";
            else if (bmi < 25m)
                category = "과체중";
            else if (bmi < 30m)
                category = "비만";
            else
                category = "고도비만";

            var result = new
            {
                UserId = userId,
                BMI = Math.Round(bmi, 2),
                Category = category,
                HeightCm = profile.HeightCm,
                WeightKg = profile.CurrentWeightKg,
                CalculatedAt = DateTime.UtcNow
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BMI 계산 중 오류 발생 - UserId: {UserId}", userId);
            return StatusCode(500, "BMI를 계산하는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 피트니스 레벨별 통계
    /// </summary>
    [HttpGet("statistics/fitness-level")]
    public async Task<ActionResult> GetFitnessLevelStatistics()
    {
        try
        {
            var statistics = await _context.Userprofiles
                .GroupBy(p => p.FitnessLevel)
                .Select(g => new
                {
                    FitnessLevel = g.Key ?? "미설정",
                    Count = g.Count(),
                    AverageAge = g.Where(p => p.BirthDate.HasValue)
                        .Average(p => DateTime.Now.Year - p.BirthDate.Value.Year),
                    AverageBMI = g.Where(p => p.HeightCm.HasValue && p.CurrentWeightKg.HasValue)
                        .Average(p =>
                            p.CurrentWeightKg!.Value / ((p.HeightCm!.Value / 100m) * (p.HeightCm.Value / 100m)))
                })
                .OrderBy(s => s.FitnessLevel)
                .ToListAsync();

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "피트니스 레벨 통계 조회 중 오류 발생");
            return StatusCode(500, "피트니스 레벨 통계를 불러오는 중 오류가 발생했습니다.");
        }
    }

    /// <summary>
    /// 피트니스 목표별 통계
    /// </summary>
    [HttpGet("statistics/fitness-goal")]
    public async Task<ActionResult> GetFitnessGoalStatistics()
    {
        try
        {
            var statistics = await _context.Userprofiles
                .GroupBy(p => p.FitnessGoal)
                .Select(g => new
                {
                    FitnessGoal = g.Key ?? "미설정",
                    Count = g.Count(),
                    Percentage = (double)g.Count() / _context.Userprofiles.Count() * 100
                })
                .OrderByDescending(s => s.Count)
                .ToListAsync();

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "피트니스 목표 통계 조회 중 오류 발생");
            return StatusCode(500, "피트니스 목표 통계를 불러오는 중 오류가 발생했습니다.");
        }
    }
}