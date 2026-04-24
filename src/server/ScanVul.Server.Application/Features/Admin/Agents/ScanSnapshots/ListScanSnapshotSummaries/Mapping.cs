using ScanVul.Server.Domain.AgentAggregate.Entities.Snapshots;

namespace ScanVul.Server.Application.Features.Admin.Agents.ScanSnapshots.ListScanSnapshotSummaries;

public static class Mapping
{
    public static ScanSnapshotSummary ToResponse(this ScanSnapshot snapshot)
    {
        return new ScanSnapshotSummary(
            SnapshotId: snapshot.Id, 
            CreatedAt: snapshot.CreatedAt,
            Payload: snapshot.Summary.ToResponse(),
            Diff: snapshot.LastDiff?.Summary.ToResponse());
    }

    private static ScanSnapshotPayloadSummary ToResponse(
        this Domain.AgentAggregate.Entities.Snapshots.ScanSnapshotSummary payload)
    {
        return new ScanSnapshotPayloadSummary(
            payload.Packages, 
            payload.VulnerablePackages, 
            payload.BduVulnerablePackages);
    }

    private static ScanSnapshotDiffSummary ToResponse(
        this Domain.AgentAggregate.Entities.Snapshots.ScanSnapshotDiffSummary d)
    {
        return new ScanSnapshotDiffSummary(
            d.AddedPackages,
            d.RemovedPackages,
            d.AddedVulnerablePackages,
            d.RemovedVulnerablePackages,
            d.AddedBduVulnerablePackages,
            d.RemovedBduVulnerablePackages);
    }
}