using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.ListBduVulnerablePackages;

/// <summary>
/// List BDU vulnerable packages request
/// </summary>
/// <param name="AgentId">Agent ID</param>
public record ListBduVulnerablePackagesRequest([FromRoute(Name = "agentId")] long AgentId);