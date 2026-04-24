using Microsoft.EntityFrameworkCore;
using ScanVul.Server.Domain.AgentAggregate.Entities.Snapshots;
using ScanVul.Server.Domain.AgentAggregate.Repositories;

namespace ScanVul.Server.Infrastructure.Data.Repositories;

public class SnapshotRepository(AppDbContext dbContext) : ISnapshotRepository
{
    public async Task<ScanSnapshot?> GetScanSnapshotByIdAsync(Guid snapshotId, bool includePayload, CancellationToken ct = default)
    {
        IQueryable<ScanSnapshot> query = dbContext.ScanSnapshots;
        if (includePayload) query = query.Include(x => x.Payload);
        
        var result = await query
            .Include(x => x.LastDiff)
            .FirstOrDefaultAsync(x => x.Id == snapshotId, ct);
        return result;
    }
}