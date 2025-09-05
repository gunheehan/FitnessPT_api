namespace FitnessPT_api.GoogleAuth.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(int userId, string email, string role);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    int GetUserIdFromToken(string token);
}