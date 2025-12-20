using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Agents.PackageInfos.ListVulnerablePackages;

/// <summary>
/// List vulnerable packages request
/// </summary>
/// <param name="AgentId">Agent ID</param>
public record ListVulnerablePackagesRequest([FromRoute(Name = "agentId")] long AgentId);