using ScanVul.Server.Domain.AgentAggregate.Entities.Snapshots;

namespace ScanVul.Server.Application.Features.Admin.Agents.ScanSnapshots.GetScanSnapshotPayload;

public static class Mapping
{
    public static GetScanSnapshotPayloadResponse ToResponse(this ScanSnapshot snapshot)
    {
        return new GetScanSnapshotPayloadResponse(Payload: snapshot.Payload?.ToResponse());
    }

    private static ScanSnapshotPayloadResponse ToResponse(this ScanSnapshotPayload payload)
    {
        return new ScanSnapshotPayloadResponse(
            Packages: payload.Packages
                .Select(x => x.ToResponse()),
            VulnerablePackages: payload.VulnerablePackages
                .Select(x => x.ToResponse()),
            BduVulnerablePackages: payload.BduVulnerablePackages
                .Select(x => x.ToResponse()));
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