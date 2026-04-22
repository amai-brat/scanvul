namespace ScanVul.Server.Domain.AgentAggregate.Entities.Snapshots;

public class ScanSnapshotPayload
{
    public Guid ScanSnapshotId { get; set; }
    
    public List<ReducedPackageInfo> Packages { get; set; } = [];
    public List<ReducedVulnerablePackage> VulnerablePackages { get; set; } = [];
    public List<ReducedVulnerablePackage> BduVulnerablePackages { get; set; } = [];

    public ScanSnapshotSummary CreateSummary()
    {
        return new ScanSnapshotSummary
        {
            Packages = Packages.Count,
            VulnerablePackages = VulnerablePackages.Count,
            BduVulnerablePackages = BduVulnerablePackages.Count
        };
    }
}

public class ScanSnapshotSummary
{
    public int Packages { get; set; }
    public int VulnerablePackages { get; set; }
    public int BduVulnerablePackages { get; set; }
}