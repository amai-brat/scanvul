using ScanVul.Server.Domain.AgentAggregate.Enums;

namespace ScanVul.Server.Domain.AgentAggregate.Entities.Snapshots;

public class ReducedPackageInfo
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Version { get; set; } = null!;

    public static ReducedPackageInfo From(PackageInfo package)
    {
        return new ReducedPackageInfo
        {
            Id = package.Id,
            Name = package.Name,
            Version = package.Version
        };
    }
}

public class ReducedVulnerablePackage
{
    public long Id { get; set; }
    public string VulnerabilityId { get; set; } = null!;
    public long PackageInfoId { get; set; }
    public string PackageName { get; set; } = null!;
    public string PackageVersion { get; set; } = null!;
    public long ComputerId { get; set; }
    public VulnerablePackageStatus Status { get; set; }

    public static ReducedVulnerablePackage From(BaseVulnerablePackage package)
    {
        return new ReducedVulnerablePackage
        {
            Id = package.Id,
            VulnerabilityId = package.VulnerabilityId,
            PackageInfoId = package.PackageInfoId,
            PackageName = package.PackageInfo.Name,
            PackageVersion = package.PackageInfo.Version,
            ComputerId = package.ComputerId,
            Status = package.Status
        };
    }
}