using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ScanVul.Server.Domain.Entities;
using ScanVul.Server.Domain.Repositories;

namespace ScanVul.Server.Infrastructure.Data.Repositories;

public class PackageInfoRepository(AppDbContext dbContext) : IPackageInfoRepository
{
    public async Task<List<PackageInfo>> GetAsync(Expression<Func<PackageInfo, bool>> filter, CancellationToken ct = default)
    {
        var entities = await dbContext.PackageInfos
            .Where(filter)
            .ToListAsync(ct);

        return entities;
    }
}