using JetBrains.Annotations;

namespace ScanVul.Server.Domain.AgentAggregate.Entities.Snapshots;

/// <summary>
/// Snapshot of packages, vulnerabilities on computer at some timestamp
/// </summary>
public class ScanSnapshot
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public long ComputerId { get; private set; }
    public Computer Computer { get; private set; } = null!;

    public ScanSnapshotPayload Payload { get; private set; } = null!;
    public ScanSnapshotSummary Summary { get; private set; } = null!;
    
    public ScanSnapshotDiff? LastDiff { get; set; }

    [UsedImplicitly]
    private ScanSnapshot() { }
    
    public ScanSnapshot(Computer computer, ScanSnapshotPayload payload)
    {
        ComputerId = computer.Id;
        Computer = computer;
        Payload = payload;
        Summary = payload.CreateSummary();
        
        CreatedAt = DateTime.UtcNow;
    }
}