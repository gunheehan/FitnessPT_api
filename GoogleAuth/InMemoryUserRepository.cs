
using FitnessPT_api.GoogleAuth.Common.Interfaces;
using FitnessPT_api.GoogleAuth.Models;
using FitnessPT_api.Models;

namespace FitnessPT_api.GoogleAuth;

/// <summary>
/// 테스트용 InMemory 사용자 저장소
/// 실제 프로젝트에서는 데이터베이스 구현체로 교체
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private static readonly List<User> _users = new();
    private static int _nextId = 1;

    public Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.GoogleId == googleId);
        return Task.FromResult(user);
    }

    public Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        user.UserId = _nextId++;
        user.CreatedAt = DateTime.UtcNow;
        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var existingUser = _users.FirstOrDefault(u => u.UserId == user.UserId);
        if (existingUser != null)
        {
            existingUser.Name = user.Name;
            existingUser.ProfileImageUrl = user.ProfileImageUrl;
            existingUser.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(existingUser);
        }
        return Task.FromResult(user);
    }
}
