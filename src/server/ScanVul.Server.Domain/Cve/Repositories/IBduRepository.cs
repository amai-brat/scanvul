using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.Cve.ValueObjects.Versions;

namespace ScanVul.Server.Domain.Cve.Repositories;

public interface IBduRepository
{
    Task<IReadOnlyCollection<BduVersionDocument>> GetMatchedBduVersionDocumentsAsync(PackageInfo packageInfo, CancellationToken ct = default);
}