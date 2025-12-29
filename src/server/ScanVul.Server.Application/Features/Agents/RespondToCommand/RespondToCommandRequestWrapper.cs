using FastEndpoints;
using ScanVul.Contracts.Agents;

namespace ScanVul.Server.Application.Features.Agents.RespondToCommand;

public record RespondToCommandRequestWrapper(Guid CommandId, string Message) : RespondToCommandRequest(CommandId, Message)
{
    [FromHeader("X-Agent-Token")] 
    public Guid AgentToken { get; init; }
}