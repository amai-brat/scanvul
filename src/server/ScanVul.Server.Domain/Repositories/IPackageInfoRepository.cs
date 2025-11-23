using System.Linq.Expressions;
using ScanVul.Server.Domain.Entities;

namespace ScanVul.Server.Domain.Repositories;

public interface IPackageInfoRepository
{
    Task<List<PackageInfo>> GetAsync(Expression<Func<PackageInfo,bool>> filter, CancellationToken ct = default);
}