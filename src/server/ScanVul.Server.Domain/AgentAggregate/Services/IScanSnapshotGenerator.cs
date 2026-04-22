using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.AgentAggregate.Entities.Snapshots;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Domain.AgentAggregate.Services;

public interface IScanSnapshotGenerator
{
    Task GenerateAsync(long computerId, CancellationToken ct = default);
}

public class ScanSnapshotGenerator(
    ILogger<ScanSnapshotGenerator> logger,
    IMemoryCache cache,
    IComputerRepository computerRepository,
    IUnitOfWork unitOfWork) : IScanSnapshotGenerator
{
    public async Task GenerateAsync(long computerId, CancellationToken ct = default)
    {
        var computer = await computerRepository.GetComputerWithAllPackagesAndLastSnapshotAsync(computerId, ct);
        if (computer == null)
        {
            logger.LogError("Computer {ComputerId} not found", computerId);
            return;
        }

        var snapshotPayload = new ScanSnapshotPayload
        {
            Packages = computer.Packages.Select(ReducedPackageInfo.From).ToList(),
            VulnerablePackages = computer.VulnerablePackages.Select(ReducedVulnerablePackage.From).ToList(),
            BduVulnerablePackages = computer.BduVulnerablePackages.Select(ReducedVulnerablePackage.From).ToList()
        };

        var scanSnapshot = new ScanSnapshot(computer, snapshotPayload);
        var lastSnapshot = computer.Snapshots.FirstOrDefault();

        if (lastSnapshot is null)
        {
            // likely this is first snapshot so add it
            computer.Snapshots.Add(scanSnapshot);
        }
        else
        {
            var diffPayload = GetDiffPayload(computerId);
            if (diffPayload.IsEmpty)
            {
                // there is last snapshot, but diff is empty => skip, no need to save same snapshot
                return;
            }
            
            computer.Snapshots.Add(scanSnapshot);
            
            // circular reference scan_snapshot.last_diff_id <-> scan_snapshot_diff.second_snapshot_id
            await unitOfWork.SaveChangesAsync(ct);
            
            var lastDiff = new ScanSnapshotDiff(lastSnapshot, scanSnapshot, diffPayload);
            scanSnapshot.LastDiff = lastDiff;
        }
        
        await unitOfWork.SaveChangesAsync(ct);

        ClearCache(computerId);
    }

    private ScanSnapshotDiffPayload GetDiffPayload(long computerId)
    {
        var diffPayload = new ScanSnapshotDiffPayload();
        
        var addedPackages = cache.Get<List<PackageInfo>>(CacheKeys.AddedPackages(computerId));
        if (addedPackages is not null) diffPayload.AddedPackages = addedPackages.Select(ReducedPackageInfo.From).ToList();
        
        var removedPackages = cache.Get<List<PackageInfo>>(CacheKeys.RemovedPackages(computerId));
        if (removedPackages is not null) diffPayload.RemovedPackages = removedPackages.Select(ReducedPackageInfo.From).ToList();
        
        var addedVulns = cache.Get<List<VulnerablePackage>>(CacheKeys.AddedVulnerablePackages(computerId));
        if (addedVulns is not null) diffPayload.AddedVulnerablePackages = addedVulns.Select(ReducedVulnerablePackage.From).ToList();
        
        var removedVulns = cache.Get<List<VulnerablePackage>>(CacheKeys.RemovedVulnerablePackages(computerId));
        if (removedVulns is not null) diffPayload.RemovedVulnerablePackages = removedVulns.Select(ReducedVulnerablePackage.From).ToList();
        
        var addedBduVulns = cache.Get<List<BduVulnerablePackage>>(CacheKeys.AddedBduVulnerablePackages(computerId));
        if (addedBduVulns is not null) diffPayload.AddedBduVulnerablePackages = addedBduVulns.Select(ReducedVulnerablePackage.From).ToList();
        
        var removedBduVulns = cache.Get<List<BduVulnerablePackage>>(CacheKeys.RemovedBduVulnerablePackages(computerId));
        if (removedBduVulns is not null) diffPayload.RemovedBduVulnerablePackages = removedBduVulns.Select(ReducedVulnerablePackage.From).ToList();

        return diffPayload;
    }

    private void ClearCache(long computerId)
    {
        cache.Remove(CacheKeys.AddedPackages(computerId));
        cache.Remove(CacheKeys.RemovedPackages(computerId));
        cache.Remove(CacheKeys.AddedVulnerablePackages(computerId));
        cache.Remove(CacheKeys.RemovedVulnerablePackages(computerId));
        cache.Remove(CacheKeys.AddedBduVulnerablePackages(computerId));
        cache.Remove(CacheKeys.RemovedBduVulnerablePackages(computerId));
    }
}