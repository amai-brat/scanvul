using JetBrains.Annotations;

namespace ScanVul.Server.Domain.AgentAggregate.Entities;

public class VulnerablePackage : BaseVulnerablePackage
{
    [UsedImplicitly]
    private VulnerablePackage() { }
    
    public VulnerablePackage(
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