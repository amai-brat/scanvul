using Microsoft.EntityFrameworkCore;
using ScanVul.Server.Domain.AgentAggregate.Entities.Snapshots;
using ScanVul.Server.Domain.AgentAggregate.Repositories;

namespace ScanVul.Server.Infrastructure.Data.Repositories;

public class SnapshotRepository(AppDbContext dbContext) : ISnapshotRepository
{
    public async Task<ScanSnapshot?> GetScanSnapshotByIdAsync(Guid snapshotId, CancellationToken ct = default)
    {
        var result = await dbContext.ScanSnapshots
            .Include(x => x.LastDiff)
            .FirstOrDefaultAsync(x => x.Id == snapshotId, ct);
        return result;
    }
}