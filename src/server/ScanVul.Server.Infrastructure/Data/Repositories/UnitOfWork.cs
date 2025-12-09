using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Infrastructure.Data.Repositories;

public class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await dbContext.SaveChangesAsync(ct);
    }
}