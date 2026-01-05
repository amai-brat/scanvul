using JetBrains.Annotations;

namespace ScanVul.Server.Domain.AgentAggregate.Entities.Commands;

public class AgentCommand
{
    public Guid Id { get; private set; }

    public long AgentId { get; private set; }
    public Agent Agent { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }
    public DateTime? SentAt { get; set; }

    public bool IsSent => SentAt.HasValue;

    public AgentCommandBody Body { get; private set; } = null!;

    public string? AgentResponse { get; set; }

    public bool IsAgentResponded => AgentResponse != null;
    
    [UsedImplicitly]
    private AgentCommand() {}

    public AgentCommand(Agent agent, AgentCommandBody body)
    {
        AgentId = agent.Id;
        Agent = agent;

        Body = body;
        CreatedAt = DateTime.UtcNow;
    }
}