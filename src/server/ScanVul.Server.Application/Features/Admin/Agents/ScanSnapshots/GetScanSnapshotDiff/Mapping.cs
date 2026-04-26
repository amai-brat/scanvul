using ScanVul.Server.Domain.AgentAggregate.Entities.Snapshots;

namespace ScanVul.Server.Application.Features.Admin.Agents.ScanSnapshots.GetScanSnapshotDiff;

public static class Mapping
{
    public static GetScanSnapshotDiffResponse ToResponse(this ScanSnapshot snapshot)
    {
        return new GetScanSnapshotDiffResponse(
            Diff: snapshot.LastDiff?.Payload.ToResponse());
    }

    private static ScanSnapshotDiffPayloadResponse ToResponse(this ScanSnapshotDiffPayload payload)
    {
        return new ScanSnapshotDiffPayloadResponse(
            payload.AddedPackages.Select(x => x.ToResponse()),
            payload.RemovedPackages.Select(x => x.ToResponse()),
            payload.AddedVulnerablePackages.Select(x => x.ToResponse()),
            payload.RemovedVulnerablePackages.Select(x => x.ToResponse()),
            payload.AddedBduVulnerablePackages.Select(x => x.ToResponse()),
            payload.RemovedBduVulnerablePackages.Select(x => x.ToResponse()));
    }
    
    private static PackageInfo ToResponse(this ReducedPackageInfo pkg)
    {
        return new PackageInfo(pkg.Id, pkg.Name, pkg.Version);
    }
    
    private static VulnerablePackage ToResponse(this ReducedVulnerablePackage vulnPkg)
    {
        return new VulnerablePackage(
            vulnPkg.Id, 
            vulnPkg.VulnerabilityId, 
            vulnPkg.PackageInfoId, 
            vulnPkg.PackageName,
            vulnPkg.PackageVersion,
            vulnPkg.Status);
    }
}