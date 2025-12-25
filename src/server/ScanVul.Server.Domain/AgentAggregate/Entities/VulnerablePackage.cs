using JetBrains.Annotations;

namespace ScanVul.Server.Domain.AgentAggregate.Entities;

public class VulnerablePackage
{
    public long Id { get; set; }
    public string CveId { get; private set; } = null!;

    public long PackageInfoId { get; private set; }
    public PackageInfo PackageInfo { get; private set; } = null!;

    public long ComputerId { get; private set; }
    public Computer Computer { get; private set; } = null!;

    public bool IsFalsePositive { get; set; } = false;

    [UsedImplicitly]
    private VulnerablePackage() { }
    
    public VulnerablePackage(
        string cveId, 
        PackageInfo packageInfo, 
        Computer computer)
    {
        CveId = cveId;
        
        PackageInfoId = packageInfo.Id;
        PackageInfo = packageInfo;
        
        ComputerId = computer.Id;
        Computer = computer;
    }
}