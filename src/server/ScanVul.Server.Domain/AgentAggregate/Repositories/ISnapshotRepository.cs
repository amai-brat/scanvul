using ScanVul.Server.Domain.AgentAggregate.Entities.Snapshots;

namespace ScanVul.Server.Domain.AgentAggregate.Repositories;

public interface ISnapshotRepository
{
    Task<ScanSnapshot?> GetScanSnapshotByIdAsync(Guid snapshotId, CancellationToken ct = default);
}