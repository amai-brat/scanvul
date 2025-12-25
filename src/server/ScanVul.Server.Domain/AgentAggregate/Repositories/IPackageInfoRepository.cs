using System.Linq.Expressions;
using ScanVul.Server.Domain.AgentAggregate.Entities;

namespace ScanVul.Server.Domain.AgentAggregate.Repositories;

public interface IPackageInfoRepository
{
    Task<List<PackageInfo>> GetAsync(Expression<Func<PackageInfo,bool>> filter, CancellationToken ct = default);
    Task<PackageInfo?> GetByIdAsync(long packageId, CancellationToken ct = default);
    Task<VulnerablePackage?> GetVulnerableByIdAsync(long vulnerablePackageId, CancellationToken ct = default);
}