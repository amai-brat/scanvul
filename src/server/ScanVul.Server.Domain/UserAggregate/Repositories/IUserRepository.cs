using ScanVul.Server.Domain.UserAggregate.Entities;

namespace ScanVul.Server.Domain.UserAggregate.Repositories;

public interface IUserRepository
{
    Task<User> AddAsync(User user, CancellationToken ct = default);
    Task<bool> IsActiveAdminExistsAsync(CancellationToken ct = default);
    Task<User?> GetByNameAsync(string name, CancellationToken ct = default);
}