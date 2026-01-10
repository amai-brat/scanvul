using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Admin.Agents.Commands.ListCommands;

/// <summary>
/// Request to get all commands of agent
/// </summary>
/// <param name="AgentId">Agent ID</param>
public record ListCommandsRequest([FromRoute(Name = "agentId")] long AgentId);