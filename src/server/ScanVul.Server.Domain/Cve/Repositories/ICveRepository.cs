using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.Cve.Entities;

namespace ScanVul.Server.Domain.Cve.Repositories;

public interface ICveRepository
{
    Task<IReadOnlyCollection<CveDocument>> GetMatchedCveDocumentsAsync(PackageInfo packageInfo, CancellationToken ct = default);
}