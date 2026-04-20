using JetBrains.Annotations;

namespace ScanVul.Server.Application.Features.Admin.Agents.ScanSnapshots.ListScanSnapshotSummaries;

/// <summary>
///  List agent's scan snapshot summaries response
/// </summary>
/// <param name="Summaries">Snapshot summaries</param>
[PublicAPI]
public record ListScanSnapshotSummariesResponse(
    List<ScanSnapshotSummary> Summaries);

/// <summary>
/// Scan snapshot summary
/// </summary>
/// <param name="SnapshotId">Snapshot ID</param>
/// <param name="Payload">Summary of snapshot payload</param>
/// <param name="Diff">Summary of diff payload between this snapshot and before (nullable)</param>
[PublicAPI]
public record ScanSnapshotSummary(
    Guid SnapshotId,
    ScanSnapshotPayloadSummary Payload,
    ScanSnapshotDiffSummary? Diff);

/// <summary>
/// Summary of snapshot payload
/// </summary>
/// <param name="Packages">Installed packages count</param>
/// <param name="VulnerablePackages">Vulnerable packages count (CVE)</param>
/// <param name="BduVulnerablePackages">Vulnerable packages count (BDU)</param>
[PublicAPI]
public record ScanSnapshotPayloadSummary(
    int Packages,
    int VulnerablePackages,
    int BduVulnerablePackages);

/// <summary>
/// Summary of diff payload between this snapshot and before
/// </summary>
/// <param name="AddedPackages">Added packages count</param>
/// <param name="RemovedPackages">Removed packages count</param>
/// <param name="AddedVulnerablePackages">Add vulnerable packages count (CVE)</param>
/// <param name="RemovedVulnerablePackages">Removed vulnerable packages count (CVE)</param>
/// <param name="AddedBduVulnerablePackages">Add vulnerable packages count (BDU)</param>
/// <param name="RemovedBduVulnerablePackages">Removed vulnerable packages count (BDU)</param>
[PublicAPI]
public record ScanSnapshotDiffSummary(
    int AddedPackages,
    int RemovedPackages,
    int AddedVulnerablePackages,
    int RemovedVulnerablePackages,
    int AddedBduVulnerablePackages,
    int RemovedBduVulnerablePackages);