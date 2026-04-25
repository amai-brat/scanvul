using Microsoft.AspNetCore.Mvc;
using ScanVul.Server.Domain.AgentAggregate.Enums;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.ListBduVulnerablePackages;

/// <summary>
/// List BDU vulnerable packages request
/// </summary>
/// <param name="AgentId">Agent ID</param>
/// <param name="Status">Filter by status (if null, get all)</param>
public record ListBduVulnerablePackagesRequest(
    [FromRoute(Name = "agentId")] long AgentId,
    [FromQuery(Name = "status")] VulnerablePackageStatus? Status);