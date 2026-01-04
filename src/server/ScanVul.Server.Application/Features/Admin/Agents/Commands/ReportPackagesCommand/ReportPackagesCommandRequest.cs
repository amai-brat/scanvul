using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Admin.Agents.Commands.ReportPackagesCommand;

/// <summary>
/// Request to 'report packages' command to agent
/// </summary>
/// <param name="AgentId">Agent ID</param>
public record ReportPackagesCommandRequest([FromRoute(Name = "agentId")] long AgentId);