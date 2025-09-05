using FitnessPT_api.GoogleAuth.Common.Interfaces;
using FitnessPT_api.GoogleAuth.Services;

namespace FitnessPT_api.GoogleAuth;

/// <summary>
/// Google 인증 모듈 - 완전히 독립적인 Google OAuth 인증 시스템
/// 다른 프로젝트에서도 재사용 가능
/// </summary>
public static class GoogleAuthModule
{
    /// <summary>
    /// Google 인증 모듈을 DI 컨테이너에 등록
    /// </summary>
    /// <param name="services">서비스 컬렉션</param>
    /// <param name="configuration">설정</param>
    /// <returns>서비스 컬렉션</returns>
    public static IServiceCollection AddGoogleAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        // HttpClient 등록 (Google API 호출용)
        services.AddHttpClient<IGoogleTokenValidator, GoogleTokenValidator>();
        
        // 서비스 등록
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        
        // 설정 검증
        ValidateConfiguration(configuration);
        
        return services;
    }
    
    private static void ValidateConfiguration(IConfiguration configuration)
    {
        var requiredKeys = new[]
        {
            "GoogleAuth:Jwt:Key",
            "GoogleAuth:Jwt:Issuer", 
            "GoogleAuth:Jwt:Audience"
        };
        
        foreach (var key in requiredKeys)
        {
            if (string.IsNullOrEmpty(configuration[key]))
            {
                throw new InvalidOperationException($"필수 설정값이 누락되었습니다: {key}");
            }
        }
    }
}