using JetBrains.Annotations;
using ScanVul.Server.Domain.AgentAggregate.Entities.Commands;

namespace ScanVul.Server.Domain.AgentAggregate.Entities;

public class Agent
{
    public long Id { get; set; }
    public Guid Token { get; init; }

    public DateTime LastPingAt { get; set; }
    public DateTime LastPackagesScrapingAt { get; set; }

    public long ComputerId { get; set; }
    public Computer Computer { get; set; } = null!;

    public List<AgentCommand> Commands { get; set; } = [];

    public bool IsActive { get; set; }

    [UsedImplicitly]
    private Agent(){}
    
    public Agent(Computer computer)
    {
        Token = Guid.CreateVersion7();
        Computer = computer;
        IsActive = true;
    }
}