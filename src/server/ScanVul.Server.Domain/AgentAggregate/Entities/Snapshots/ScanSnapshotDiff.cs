using JetBrains.Annotations;

namespace ScanVul.Server.Domain.AgentAggregate.Entities.Snapshots;

public class ScanSnapshotDiff
{
    public Guid Id { get; private set; }
    
    public Guid FirstSnapshotId { get; private set; }
    public ScanSnapshot FirstSnapshot { get; private set; } = null!;

    public Guid SecondSnapshotId { get; private set; }
    public ScanSnapshot SecondSnapshot { get; private set; } = null!;

    public ScanSnapshotDiffPayload Payload { get; private set; } = null!;

    [UsedImplicitly]
    private ScanSnapshotDiff() { }
    
    public ScanSnapshotDiff(
        ScanSnapshot firstSnapshot, 
        ScanSnapshot secondSnapshot,
        ScanSnapshotDiffPayload payload)
    {
        FirstSnapshotId = firstSnapshot.Id;
        FirstSnapshot = firstSnapshot;
        SecondSnapshotId = secondSnapshot.Id;
        SecondSnapshot = secondSnapshot;
        Payload = payload;
    }
}

public class ScanSnapshotDiffPayload
{
    public bool IsEmpty => AddedPackages.Count == 0 && 
                           RemovedPackages.Count == 0 && 
                           AddedVulnerablePackages.Count == 0 &&
                           RemovedVulnerablePackages.Count == 0 &&
                           AddedBduVulnerablePackages.Count == 0 &&
                           RemovedBduVulnerablePackages.Count == 0;
    
    public List<PackageInfo> AddedPackages { get; set; } = [];
    public List<PackageInfo> RemovedPackages { get; set; } = [];
    
    public List<VulnerablePackage> AddedVulnerablePackages { get; set; } = [];
    public List<VulnerablePackage> RemovedVulnerablePackages { get; set; } = [];
    
    public List<BduVulnerablePackage> AddedBduVulnerablePackages { get; set; } = [];
    public List<BduVulnerablePackage> RemovedBduVulnerablePackages { get; set; } = [];
}