using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Admin.Agents.ScanSnapshots.GetScanSnapshotDiff;

/// <summary>
/// Get scan snapshot last diff request
/// </summary>
/// <param name="SnapshotId">Snaphot ID</param>
[PublicAPI]
public record GetScanSnapshotDiffRequest(
    [FromRoute(Name = "snapshotId")] Guid SnapshotId);