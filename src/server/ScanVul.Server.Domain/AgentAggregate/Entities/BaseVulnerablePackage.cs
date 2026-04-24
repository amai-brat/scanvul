using ScanVul.Server.Domain.AgentAggregate.Enums;

namespace ScanVul.Server.Domain.AgentAggregate.Entities;

public abstract class BaseVulnerablePackage
{
    /// <summary>
    /// Vulnerable package ID
    /// </summary>
    public long Id { get; protected set; }

    /// <summary>
    /// Vulnerability ID (e.g. CVE-2024-56738, BDU:2026-05547)
    /// </summary>
    public string VulnerabilityId { get; protected set; } = null!;
    
    /// <summary>
    /// Package ID
    /// </summary>
    public long PackageInfoId { get; protected set; }
    
    /// <summary>
    /// Package
    /// </summary>
    public PackageInfo PackageInfo { get; protected set; } = null!;
    
    /// <summary>
    /// ID of computer with package
    /// </summary>
    public long ComputerId { get; protected set; }
    
    /// <summary>
    /// Computer with package
    /// </summary>
    public Computer Computer { get; protected set; } = null!;
    
    /// <summary>
    /// Status
    /// </summary>
    public VulnerablePackageStatus Status { get; set; }
}