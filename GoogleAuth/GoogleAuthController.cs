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
    /// **Google ID Token 획득 방법:**
    /// 
    /// 1. **Google Developers Console**에서 OAuth 클라이언트 ID 생성
    /// 2. **테스트용 HTML 페이지** 생성:
    /// 
    /// ```html
    /// &lt;script src="https://accounts.google.com/gsi/client"&gt;&lt;/script&gt;
    /// &lt;div id="g_id_onload" data-client_id="YOUR_CLIENT_ID" data-callback="handleCredentialResponse"&gt;&lt;/div&gt;
    /// &lt;div class="g_id_signin"&gt;&lt;/div&gt;
    /// &lt;script&gt;
    /// function handleCredentialResponse(response) {
    ///     console.log("Google ID Token: " + response.credential);
    ///     // 이 토큰을 아래 API에 전송
    /// }
    /// &lt;/script&gt;
    /// ```
    /// 
    /// 3. **또는 JWT.io**에서 샘플 토큰 생성 가능
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
    /// **사용 방법:**
    /// 1. `/login` 엔드포인트에서 JWT 토큰 획득
    /// 2. Swagger 상단의 🔒 버튼 클릭
    /// 3. `Bearer {획득한_accessToken}` 형식으로 입력
    /// 4. 이 엔드포인트 호출
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

    /// <summary>
    /// 📊 Google 인증 통계 정보
    /// </summary>
    /// <returns>현재 메모리에 저장된 사용자 통계</returns>
    [HttpGet("stats")]
    public ActionResult<object> GetAuthStats()
    {
        // InMemoryUserRepository에서 통계 정보 가져오기
        // 실제 DB 구현 시에는 DB에서 통계 조회
        
        return Ok(new
        {
            TotalUsers = "N/A (InMemory 구현)",
            NewUsersToday = "N/A (InMemory 구현)",
            ActiveSessions = "N/A (JWT는 stateless)",
            LastLoginTime = DateTime.UtcNow,
            Message = "현재는 InMemory 저장소를 사용하므로 실제 통계는 DB 연동 후 제공됩니다."
        });
    }

    /// <summary>
    /// 🛠️ Google 토큰 디버깅 도구
    /// </summary>
    /// <param name="request">Google ID Token</param>
    /// <returns>토큰 정보 분석 결과</returns>
    /// <remarks>
    /// 개발용 디버깅 도구입니다. Google ID Token의 구조를 분석합니다.
    /// 
    /// **주의:** 실제 로그인은 수행하지 않고 토큰 구조만 분석합니다.
    /// </remarks>
    [HttpPost("debug-token")]
    public async Task<ActionResult<object>> DebugGoogleToken(
        [FromBody] GoogleAuthRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Google 토큰 검증만 수행 (실제 로그인은 안 함)
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={request.GoogleToken}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new
                {
                    Error = "Invalid Google Token",
                    StatusCode = response.StatusCode,
                    Message = "Google에서 토큰을 인식하지 못했습니다."
                });
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            return Ok(new
            {
                Message = "Google 토큰 분석 완료",
                TokenValid = true,
                GoogleResponse = System.Text.Json.JsonSerializer.Deserialize<object>(jsonContent),
                Note = "이것은 디버깅용이며 실제 로그인은 /login 엔드포인트를 사용하세요."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                Error = "Token Analysis Failed",
                Message = ex.Message,
                Note = "토큰 형식이 올바르지 않거나 만료되었을 수 있습니다."
            });
        }
    }
}