
using FitnessPT_api.GoogleAuth.Models;

namespace FitnessPT_api.GoogleAuth.Common.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleAuthResult> AuthenticateAsync(string googleIdToken, CancellationToken cancellationToken = default);
    Task<bool> ValidateJwtTokenAsync(string jwtToken, CancellationToken cancellationToken = default);
    int GetUserIdFromToken(string jwtToken);
}