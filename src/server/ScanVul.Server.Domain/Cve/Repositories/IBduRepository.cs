using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.Cve.ValueObjects.Descriptions;
using ScanVul.Server.Domain.Cve.ValueObjects.Versions;

namespace ScanVul.Server.Domain.Cve.Repositories;

public interface IBduRepository
{
    Task<IReadOnlyCollection<BduVersionDocument>> GetMatchedBduVersionDocumentsAsync(PackageInfo packageInfo, CancellationToken ct = default);
    Task<IEnumerable<BduDescriptionDocument>> GetBduDescriptionDocumentsAsync(IEnumerable<string> bduIds, CancellationToken ct = default);
}