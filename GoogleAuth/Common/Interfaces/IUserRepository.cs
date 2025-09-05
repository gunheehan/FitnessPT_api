using FitnessPT_api.GoogleAuth.Models;

namespace FitnessPT_api.GoogleAuth.Common.Interfaces;

public interface IUserRepository
{
    Task<GoogleUser?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default);
    Task<GoogleUser> CreateAsync(GoogleUser user, CancellationToken cancellationToken = default);
    Task<GoogleUser> UpdateAsync(GoogleUser user, CancellationToken cancellationToken = default);
}