using JetBrains.Annotations;

namespace ScanVul.Server.Domain.AgentAggregate.Entities;

/// <summary>
/// Vulnerable package from БДУ
/// </summary>
public class BduVulnerablePackage : BaseVulnerablePackage
{
    [UsedImplicitly]
    private BduVulnerablePackage() { }
    
    public BduVulnerablePackage(
        string vulnerabilityId, 
        PackageInfo packageInfo, 
        Computer computer)
    {
        VulnerabilityId = vulnerabilityId;
        
        PackageInfoId = packageInfo.Id;
        PackageInfo = packageInfo;
        
        ComputerId = computer.Id;
        Computer = computer;
    }
}