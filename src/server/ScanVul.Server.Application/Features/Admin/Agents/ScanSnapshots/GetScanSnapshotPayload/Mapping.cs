using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.AgentAggregate.Entities.Snapshots;

namespace ScanVul.Server.Application.Features.Admin.Agents.ScanSnapshots.GetScanSnapshotPayload;

public static class Mapping
{
    public static GetScanSnapshotPayloadResponse ToResponse(this ScanSnapshot snapshot, bool includePayload)
    {
        return new GetScanSnapshotPayloadResponse(
            Payload: includePayload 
                ? snapshot.Payload!.ToResponse() 
                : null,
            Diff: snapshot.LastDiff?.Payload.ToResponse());
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
            vulnPkg.IsFalsePositive, 
            vulnPkg.IsPatchless);
    }
}