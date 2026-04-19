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
            Packages = computer.Packages,
            VulnerablePackages = computer.VulnerablePackages,
            BduVulnerablePackages = computer.BduVulnerablePackages
        };

        var scanSnapshot = new ScanSnapshot(computer, snapshotPayload);

        var lastSnapshot = computer.Snapshots.FirstOrDefault();
        if (lastSnapshot is not null)
        {
            var diffPayload = GetDiffPayload(computerId);
            var lastDiff = new ScanSnapshotDiff(lastSnapshot, scanSnapshot, diffPayload);
            scanSnapshot.LastDiff = lastDiff;
        }
        
        computer.Snapshots.Add(scanSnapshot);
        await unitOfWork.SaveChangesAsync(ct);

        ClearCache(computerId);
    }

    private ScanSnapshotDiffPayload GetDiffPayload(long computerId)
    {
        var diffPayload = new ScanSnapshotDiffPayload();
        
        var addedPackages = cache.Get<List<PackageInfo>>(CacheKeys.AddedPackages(computerId));
        if (addedPackages is not null) diffPayload.AddedPackages = addedPackages;
        
        var removedPackages = cache.Get<List<PackageInfo>>(CacheKeys.RemovedPackages(computerId));
        if (removedPackages is not null) diffPayload.RemovedPackages = removedPackages;
        
        var addedVulns = cache.Get<List<VulnerablePackage>>(CacheKeys.AddedVulnerablePackages(computerId));
        if (addedVulns is not null) diffPayload.AddedVulnerablePackages = addedVulns;
        
        var removedVulns = cache.Get<List<VulnerablePackage>>(CacheKeys.RemovedVulnerablePackages(computerId));
        if (removedVulns is not null) diffPayload.RemovedVulnerablePackages = removedVulns;
        
        var addedBduVulns = cache.Get<List<BduVulnerablePackage>>(CacheKeys.AddedBduVulnerablePackages(computerId));
        if (addedBduVulns is not null) diffPayload.AddedBduVulnerablePackages = addedBduVulns;
        
        var removedBduVulns = cache.Get<List<BduVulnerablePackage>>(CacheKeys.RemovedBduVulnerablePackages(computerId));
        if (removedBduVulns is not null) diffPayload.RemovedBduVulnerablePackages = removedBduVulns;

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