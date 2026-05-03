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
    public ScanSnapshotDiffSummary Summary { get; private set; } = null!;

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
        Summary = payload.CreateSummary();
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
    
    public List<ReducedPackageInfo> AddedPackages { get; set; } = [];
    public List<ReducedPackageInfo> RemovedPackages { get; set; } = [];
    
    public List<ReducedVulnerablePackage> AddedVulnerablePackages { get; set; } = [];
    public List<ReducedVulnerablePackage> RemovedVulnerablePackages { get; set; } = [];
    
    public List<ReducedVulnerablePackage> AddedBduVulnerablePackages { get; set; } = [];
    public List<ReducedVulnerablePackage> RemovedBduVulnerablePackages { get; set; } = [];

    public ScanSnapshotDiffSummary CreateSummary()
    {
        return new ScanSnapshotDiffSummary
        {
            AddedPackages = AddedPackages.Count,
            RemovedPackages = RemovedPackages.Count,
            AddedVulnerablePackages = AddedVulnerablePackages.Count,
            RemovedVulnerablePackages = RemovedVulnerablePackages.Count,
            AddedBduVulnerablePackages = AddedBduVulnerablePackages.Count,
            RemovedBduVulnerablePackages = RemovedBduVulnerablePackages.Count
        };
    }
}

public class ScanSnapshotDiffSummary
{
    public int AddedPackages { get; set; }
    public int RemovedPackages { get; set; }
    
    public int AddedVulnerablePackages { get; set; }
    public int RemovedVulnerablePackages { get; set; }
    
    public int AddedBduVulnerablePackages { get; set; }
    public int RemovedBduVulnerablePackages { get; set; }
}