using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.ListPackages;

/// <summary>
/// List packages request
/// </summary>
/// <param name="AgentId">Agent ID</param>
public record ListPackagesRequest([FromRoute(Name = "agentId")] long AgentId);
