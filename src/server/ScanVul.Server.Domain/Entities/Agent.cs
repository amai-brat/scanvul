using JetBrains.Annotations;

namespace ScanVul.Server.Domain.Entities;

public class Agent
{
    public long Id { get; set; }
    public Guid Token { get; }

    public DateTime LastPingAt { get; set; }
    public DateTime LastPackagesScrapingAt { get; set; }

    public long ComputerId { get; set; }
    public Computer Computer { get; set; } = null!;

    [UsedImplicitly]
    private Agent(){}
    
    public Agent(Computer computer)
    {
        Token = Guid.CreateVersion7();
        Computer = computer;
    }
}