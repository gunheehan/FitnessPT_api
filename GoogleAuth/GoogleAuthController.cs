using FitnessPT_api.GoogleAuth.Common.Interfaces;
using FitnessPT_api.GoogleAuth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessPT_api.GoogleAuth;

/// <summary>
/// Google OAuth 인증 전용 컨트롤러
/// Google 로그인 및 JWT 토큰 관리
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Google Authentication")]
public class GoogleAuthController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly ILogger<GoogleAuthController> _logger;

    public GoogleAuthController(IGoogleAuthService googleAuthService, ILogger<GoogleAuthController> logger)
    {
        _googleAuthService = googleAuthService;
        _logger = logger;
    }

    /// <summary>
    /// 🔍 API 연결 상태 확인
    /// </summary>
    /// <returns>API 상태 정보</returns>
    [HttpGet("status")]
    public ActionResult<object> GetStatus()
    {
        return Ok(new
        {
            Service = "Google Authentication API",
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Message = "Google OAuth 인증 서비스가 정상 작동 중입니다."
        });
    }

    /// <summary>
    /// 🔐 Google OAuth 로그인
    /// </summary>
    /// <param name="request">Google ID Token</param>
    /// <param name="cancellationToken"></param>
    /// <returns>JWT 토큰 및 사용자 정보</returns>
    /// <remarks>
    /// 
    /// **응답 예시:**
    /// ```json
    /// {
    ///   "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    ///   "refreshToken": "base64-encoded-refresh-token",
    ///   "expiresAt": "2024-01-15T12:00:00Z",
    ///   "user": {
    ///     "id": 1,
    ///     "googleId": "google-user-id",
    ///     "email": "user@example.com",
    ///     "name": "사용자 이름",
    ///     "role": "Member",
    ///     "isNewUser": true
    ///   },
    ///   "success": true
    /// }
    /// ```
    /// </remarks>
    [HttpPost("login")]
    public async Task<ActionResult<GoogleAuthResult>> GoogleLogin(
        [FromBody] GoogleAuthRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new GoogleAuthResult
                {
                    Success = false,
                    ErrorMessage = "Google 토큰이 필요합니다."
                });
            }

            var result = await _googleAuthService.AuthenticateAsync(request.GoogleToken, cancellationToken);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            _logger.LogInformation("Google 로그인 성공: {Email}, 신규사용자: {IsNewUser}", 
                result.User.Email, result.IsNewUser);
                
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Google 인증 실패");
            return Unauthorized(new GoogleAuthResult
            {
                Success = false,
                ErrorMessage = "Google 인증에 실패했습니다: " + ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google 로그인 중 오류 발생");
            return StatusCode(500, new GoogleAuthResult
            {
                Success = false,
                ErrorMessage = "로그인 처리 중 오류가 발생했습니다: " + ex.Message
            });
        }
    }

    /// <summary>
    /// 🔑 JWT 토큰 검증 테스트
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>현재 인증된 사용자 정보</returns>
    /// <remarks>
    /// 
    /// **응답 예시:**
    /// ```json
    /// {
    ///   "userId": "1",
    ///   "email": "user@example.com",
    ///   "role": "Member",
    ///   "authProvider": "google",
    ///   "isAuthenticated": true,
    ///   "tokenInfo": {
    ///     "issuer": "FitnessPT.Api",
    ///     "audience": "FitnessPT.Client",
    ///     "expiresAt": "2024-01-15T12:00:00Z"
    ///   }
    /// }
    /// ```
    /// </remarks>
    [HttpGet("verify-token")]
    [Authorize]
    public ActionResult<object> VerifyToken(CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var emailClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var jtiClaim = User.FindFirst("jti")?.Value;
            var authProviderClaim = User.FindFirst("auth_provider")?.Value;

            return Ok(new
            {
                UserId = userIdClaim,
                Email = emailClaim,
                Role = roleClaim,
                AuthProvider = authProviderClaim,
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                TokenInfo = new
                {
                    Jti = jtiClaim,
                    Issuer = User.FindFirst("iss")?.Value,
                    Audience = User.FindFirst("aud")?.Value,
                    ExpiresAt = User.FindFirst("exp")?.Value != null 
                        ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(User.FindFirst("exp")!.Value)).DateTime
                        : (DateTime?)null
                },
                Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "토큰 검증 중 오류 발생");
            return StatusCode(500, new
            {
                Error = "토큰 검증 중 오류가 발생했습니다.",
                Message = ex.Message
            });
        }
    }
}