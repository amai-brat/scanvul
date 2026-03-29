using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.ScanPackages;

/// <summary>
/// Scan packages of agent request
/// </summary>
/// <param name="AgentId">Agent ID</param>
public record ScanPackagesRequest([FromRoute(Name = "agentId")] long AgentId);