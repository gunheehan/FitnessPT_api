using FitnessPT_api.Data;
using FitnessPT_api.GoogleAuth.Common.Interfaces;
using FitnessPT_api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessPT_api.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext context;
    
    public UserRepository(AppDbContext _context)
    {
        context = _context;
    }
    
    public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .SingleOrDefaultAsync(u => u.GoogleId == googleId, cancellationToken);

        return user;
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        var exists = await context.Users
            .AnyAsync(u => u.GoogleId == user.GoogleId, cancellationToken);
    
        if (exists)
            throw new InvalidOperationException($"이미 존재하는 계정입니다: {user.Email}");

        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        user.IsActive = true;

        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var existingUser = await context.Users.FindAsync(new object[] { user.UserId }, cancellationToken);
    
        if (existingUser == null)
        {
            throw new InvalidOperationException($"사용자를 찾을 수 없습니다. ID: {user.Email}");
        }

        existingUser.Name = user.Name;
        existingUser.ProfileImageUrl = user.ProfileImageUrl;
        existingUser.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return user;
    }
}