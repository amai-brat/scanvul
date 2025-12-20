using Microsoft.EntityFrameworkCore;
using ScanVul.Server.Domain.UserAggregate.Entities;
using ScanVul.Server.Domain.UserAggregate.Repositories;

namespace ScanVul.Server.Infrastructure.Data.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        var entry = dbContext.Users.Add(user);
        return Task.FromResult(entry.Entity);
    }

    public async Task<bool> IsActiveAdminExistsAsync(CancellationToken ct = default)
    {
        return await dbContext.Users.AnyAsync(x => x.IsActive, cancellationToken: ct);
    }

    public async Task<User?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Name == name, ct);
        return user;
    }
}