using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Agents.Commands.ReportPackagesCommand;

/// <summary>
/// Request to send report packages command to agent
/// </summary>
/// <param name="AgentId">Agent ID</param>
public record ReportPackagesCommandRequest([FromRoute(Name = "agentId")] long AgentId);