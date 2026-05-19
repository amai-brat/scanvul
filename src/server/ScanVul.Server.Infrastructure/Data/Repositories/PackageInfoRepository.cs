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
            .Include(x => x.PackageInfo)
            .FirstOrDefaultAsync(x => x.Id == vulnerablePackageId, ct);
        
        return package;
    }

    public async Task<IReadOnlyCollection<VulnerablePackage>> GetVulnerableByIdsAsync(IReadOnlyCollection<long> vulnerablePackageIds, CancellationToken ct = default)
    {
        var packages = await dbContext.VulnerablePackages
            .Include(x => x.PackageInfo)
            .Where(x => vulnerablePackageIds.Contains(x.Id))
            .ToListAsync(cancellationToken: ct);

        return packages;
    }

    public async Task<BduVulnerablePackage?> GetBduVulnerableByIdAsync(long vulnerablePackageId, CancellationToken ct = default)
    {
        var package = await dbContext.BduVulnerablePackages
            .Include(x => x.PackageInfo)
            .FirstOrDefaultAsync(x => x.Id == vulnerablePackageId, ct);
        
        return package;
    }
    
    public async Task<IReadOnlyCollection<BduVulnerablePackage>> GetBduVulnerableByIdsAsync(IReadOnlyCollection<long> vulnerablePackageIds, CancellationToken ct = default)
    {
        var packages = await dbContext.BduVulnerablePackages
            .Include(x => x.PackageInfo)
            .Where(x => vulnerablePackageIds.Contains(x.Id))
            .ToListAsync(cancellationToken: ct);

        return packages;
    }
}