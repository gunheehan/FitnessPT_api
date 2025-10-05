

using FitnessPT_api.GoogleAuth.Common.Interfaces;
using FitnessPT_api.GoogleAuth.Models;
using FitnessPT_api.Models;

namespace FitnessPT_api.GoogleAuth.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IGoogleTokenValidator _tokenValidator;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _userRepository; // User DB 연동
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(
        IGoogleTokenValidator tokenValidator,
        IJwtTokenService jwtTokenService,
        IUserRepository userRepository,
        ILogger<GoogleAuthService> logger)
    {
        _tokenValidator = tokenValidator;
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<GoogleAuthResult> AuthenticateAsync(string googleIdToken, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Google 토큰 검증
            var googleTokenInfo = await _tokenValidator.ValidateAsync(googleIdToken, cancellationToken);
            
            // 2. 기존 사용자 확인
            var existingUser = await _userRepository.GetByGoogleIdAsync(googleTokenInfo.GoogleId, cancellationToken);
            
            User user;
            bool isNewUser = false;
            
            if (existingUser == null)
            {
                // 3. 신규 사용자 생성
                user = new User
                {
                    GoogleId = googleTokenInfo.GoogleId,
                    Email = googleTokenInfo.Email,
                    Name = googleTokenInfo.Name,
                    ProfileImageUrl = googleTokenInfo.Picture,
                    Role = "Member",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                user = await _userRepository.CreateAsync(user, cancellationToken);
                isNewUser = true;
                
                _logger.LogInformation("신규 사용자 생성: {Email} (GoogleId: {GoogleId})", 
                    googleTokenInfo.Email, googleTokenInfo.GoogleId);
            }
            else
            {
                user = existingUser;
                
                // 프로필 정보 업데이트 (Google에서 변경된 경우)
                bool needsUpdate = false;
                if (user.Name != googleTokenInfo.Name)
                {
                    user.Name = googleTokenInfo.Name;
                    needsUpdate = true;
                }
                
                if (user.ProfileImageUrl != googleTokenInfo.Picture)
                {
                    user.ProfileImageUrl = googleTokenInfo.Picture;
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    user.UpdatedAt = DateTime.UtcNow;
                    user = await _userRepository.UpdateAsync(user, cancellationToken);
                    _logger.LogInformation("사용자 프로필 업데이트: {Email}", googleTokenInfo.Email);
                }
                
                _logger.LogInformation("기존 사용자 로그인: {Email}", googleTokenInfo.Email);
            }

            // 4. JWT 토큰 생성
            var accessToken = _jwtTokenService.GenerateAccessToken(user.UserId, user.Email, user.Role);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            return new GoogleAuthResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = user,
                IsNewUser = isNewUser,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google 인증 중 오류 발생");
            return new GoogleAuthResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> ValidateJwtTokenAsync(string jwtToken, CancellationToken cancellationToken = default)
    {
        return _jwtTokenService.ValidateToken(jwtToken);
    }

    public int GetUserIdFromToken(string jwtToken)
    {
        return _jwtTokenService.GetUserIdFromToken(jwtToken);
    }
}