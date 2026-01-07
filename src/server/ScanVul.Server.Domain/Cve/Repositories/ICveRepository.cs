using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.Cve.ValueObjects.Descriptions;
using ScanVul.Server.Domain.Cve.ValueObjects.Versions;

namespace ScanVul.Server.Domain.Cve.Repositories;

public interface ICveRepository
{
    Task<IReadOnlyCollection<CveVersionDocument>> GetMatchedCveVersionDocumentsAsync(PackageInfo packageInfo, CancellationToken ct = default);
    Task<IEnumerable<CveDescriptionDocument>> GetCveDescriptionDocumentsAsync(IEnumerable<string> cveIds, CancellationToken ct = default);
}