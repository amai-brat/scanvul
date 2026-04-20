using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Admin.Agents.ScanSnapshots.GetScanSnapshotPayload;

/// <summary>
/// Get scan snapshot payload request
/// </summary>
/// <param name="SnapshotId">Snaphot ID</param>
/// <param name="IncludePayload">Return with snapshot payload (if false, only diff returned if exists)</param>
[PublicAPI]
public record GetScanSnapshotPayloadRequest(
    [FromRoute(Name = "snapshotId")] Guid SnapshotId,
    [FromQuery(Name = "includePayload")] bool IncludePayload = false);