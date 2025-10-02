using FitnessPT_api.GoogleAuth.Models;
using FitnessPT_api.Models;

namespace FitnessPT_api.GoogleAuth.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);
}