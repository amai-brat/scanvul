using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Admin.Agents.ScanSnapshots.ListScanSnapshotSummaries;

/// <summary>
/// List agent's scan snapshot summaries request
/// </summary>
/// <param name="AgentId">Agent ID</param>
[PublicAPI]
public record ListScanSnapshotSummariesRequest(
    [FromRoute(Name = "agentId")] long AgentId);