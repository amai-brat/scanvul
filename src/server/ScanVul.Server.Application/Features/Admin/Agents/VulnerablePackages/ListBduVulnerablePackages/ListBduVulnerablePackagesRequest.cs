using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.ListBduVulnerablePackages;

/// <summary>
/// List BDU vulnerable packages request
/// </summary>
/// <param name="AgentId">Agent ID</param>
/// <param name="IsFalsePositive">Filter by false-positive (if null, get all)</param>
/// <param name="IsPatchless">Filter by patchless (if null, get all)</param>
public record ListBduVulnerablePackagesRequest(
    [FromRoute(Name = "agentId")] long AgentId,
    [FromQuery(Name = "isFalsePositive")] bool? IsFalsePositive,
    [FromQuery(Name = "isPatchless")] bool? IsPatchless);