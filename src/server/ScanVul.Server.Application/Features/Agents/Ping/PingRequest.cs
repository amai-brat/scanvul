using FastEndpoints;

namespace ScanVul.Server.Application.Features.Agents.Ping;

public class PingRequest
{
    [FromHeader("X-Agent-Token")] 
    public Guid AgentToken { get; init; }
}