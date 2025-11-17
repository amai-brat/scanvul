namespace ScanVul.Server.Domain.Entities;

public class Agent
{
    public long Id { get; set; }
    public Guid Token { get; }

    public DateTime LastPingAt { get; set; }
    public DateTime LastPackagesScrapingAt { get; set; }

    public Computer Computer { get; set; }

    public Agent(Computer computer)
    {
        Token = Guid.CreateVersion7();
        Computer = computer;
    }
}