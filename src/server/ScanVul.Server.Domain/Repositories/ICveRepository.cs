using ScanVul.Server.Domain.Entities;

namespace ScanVul.Server.Domain.Repositories;

public interface ICveRepository
{
    Task<IReadOnlyCollection<CveDocument>> GetMatchedCveDocumentsAsync(PackageInfo packageInfo, CancellationToken ct = default);
}