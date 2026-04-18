using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.ListVulnerablePackages;

/// <summary>
/// List vulnerable packages request
/// </summary>
/// <param name="AgentId">Agent ID</param>
/// <param name="IsFalsePositive">Filter by false-positive (if null, get all)</param>
/// <param name="IsPatchless">Filter by patchless (if null, get all)</param>
public record ListVulnerablePackagesRequest(
    [FromRoute(Name = "agentId")] long AgentId, 
    [FromQuery(Name = "isFalsePositive")] bool? IsFalsePositive,
    [FromQuery(Name = "isPatchless")] bool? IsPatchless);