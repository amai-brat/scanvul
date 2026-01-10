using JetBrains.Annotations;

namespace ScanVul.Server.Application.Features.Admin.Agents.Commands.ListCommands;

/// <summary>
/// List commands response
/// </summary>
/// <param name="Commands"></param>
[PublicAPI]
public record ListCommandsResponse(List<CommandResponse> Commands);

/// <summary>
/// Agent command response
/// </summary>
/// <param name="Id">Command ID</param>
/// <param name="Type">Command type</param>
/// <param name="CreatedAt">Created at timestamp (UTC)</param>
/// <param name="SentAt">Sent at to agent timestamp (UTC)</param>
/// <param name="AgentResponse">Agent response to command</param>
/// <param name="CommandParams">Parameters of command</param>
[PublicAPI]
public record CommandResponse(
    Guid Id,
    string Type,
    DateTime CreatedAt,
    DateTime? SentAt,
    string? AgentResponse,
    object CommandParams);