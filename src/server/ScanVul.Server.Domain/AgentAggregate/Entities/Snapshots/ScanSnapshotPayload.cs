namespace ScanVul.Server.Domain.AgentAggregate.Entities.Snapshots;

public class ScanSnapshotPayload
{
    public List<PackageInfo> Packages { get; set; } = [];
    public List<VulnerablePackage> VulnerablePackages { get; set; } = [];
    public List<BduVulnerablePackage> BduVulnerablePackages { get; set; } = [];

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