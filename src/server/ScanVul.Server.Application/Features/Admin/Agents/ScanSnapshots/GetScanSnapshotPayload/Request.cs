using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Admin.Agents.ScanSnapshots.GetScanSnapshotPayload;

/// <summary>
/// Get scan snapshot payload request
/// </summary>
/// <param name="SnapshotId">Snaphot ID</param>
[PublicAPI]
public record GetScanSnapshotPayloadRequest(
    [FromRoute(Name = "snapshotId")] Guid SnapshotId);