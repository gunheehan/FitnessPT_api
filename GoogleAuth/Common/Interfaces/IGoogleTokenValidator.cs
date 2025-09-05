using FitnessPT_api.GoogleAuth.Models;

namespace FitnessPT_api.GoogleAuth.Common.Interfaces;

public interface IGoogleTokenValidator
{
    Task<GoogleTokenInfo> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}