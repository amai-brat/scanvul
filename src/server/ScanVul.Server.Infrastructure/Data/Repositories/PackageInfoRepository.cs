using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.AgentAggregate.Repositories;

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

    public async Task<PackageInfo?> GetByIdAsync(long packageId, CancellationToken ct = default)
    {
        var package = await dbContext.PackageInfos
            .FirstOrDefaultAsync(x => x.Id == packageId, ct);
        
        return package;
    }

    public async Task<VulnerablePackage?> GetVulnerableByIdAsync(long vulnerablePackageId, CancellationToken ct = default)
    {
        var package = await dbContext.VulnerablePackages
            .FirstOrDefaultAsync(x => x.Id == vulnerablePackageId, ct);
        
        return package;
    }
}