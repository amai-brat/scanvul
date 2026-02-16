using JetBrains.Annotations;

namespace ScanVul.Server.Domain.AgentAggregate.Entities;

/// <summary>
/// Vulnerable package from БДУ
/// </summary>
public class BduVulnerablePackage
{
    public long Id { get; set; }
    public string BduId { get; private set; } = null!;

    public long PackageInfoId { get; private set; }
    public PackageInfo PackageInfo { get; private set; } = null!;

    public long ComputerId { get; private set; }
    public Computer Computer { get; private set; } = null!;
    
    public bool IsFalsePositive { get; set; } = false;
    
    [UsedImplicitly]
    private BduVulnerablePackage() { }
    
    public BduVulnerablePackage(
        string bduId, 
        PackageInfo packageInfo, 
        Computer computer)
    {
        BduId = bduId;
        
        PackageInfoId = packageInfo.Id;
        PackageInfo = packageInfo;
        
        ComputerId = computer.Id;
        Computer = computer;
    }
}