using System.Text.Json;
using FitnessPT_api.GoogleAuth.Common.Interfaces;
using FitnessPT_api.GoogleAuth.Models;

namespace FitnessPT_api.GoogleAuth.Services;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleTokenValidator> _logger;

    public GoogleTokenValidator(HttpClient httpClient, ILogger<GoogleTokenValidator> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GoogleTokenInfo> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Google token validation failed with status: {StatusCode}", response.StatusCode);
                throw new UnauthorizedAccessException("Google 토큰 검증에 실패했습니다.");
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (tokenResponse == null)
            {
                throw new InvalidOperationException("Google 응답을 파싱할 수 없습니다.");
            }

            return new GoogleTokenInfo
            {
                GoogleId = tokenResponse.Sub,
                Email = tokenResponse.Email,
                Name = tokenResponse.Name,
                Picture = tokenResponse.Picture,
                EmailVerified = tokenResponse.EmailVerified == "true"
            };
        }
        catch (Exception ex) when (!(ex is UnauthorizedAccessException))
        {
            _logger.LogError(ex, "Google 토큰 검증 중 오류 발생");
            throw new InvalidOperationException("토큰 검증 중 오류가 발생했습니다.", ex);
        }
    }

    private class GoogleTokenResponse
    {
        public string Sub { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public string EmailVerified { get; set; } = string.Empty;
    }
}