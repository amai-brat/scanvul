namespace ScanVul.Server.Domain.AgentAggregate.Entities.Snapshots;

public class ScanSnapshotPayload
{
    public List<PackageInfo> Packages { get; set; } = [];
    public List<VulnerablePackage> VulnerablePackages { get; set; } = [];
    public List<BduVulnerablePackage> BduVulnerablePackages { get; set; } = [];
}