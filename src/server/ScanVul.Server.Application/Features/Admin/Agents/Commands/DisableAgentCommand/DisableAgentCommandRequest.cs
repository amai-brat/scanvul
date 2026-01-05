using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Admin.Agents.Commands.DisableAgentCommand;

/// <summary>
/// Request to 'disable agent' command to agent
/// </summary>
/// <param name="AgentId">Agent ID</param>
public record DisableAgentCommandRequest([FromRoute(Name = "agentId")] long AgentId);