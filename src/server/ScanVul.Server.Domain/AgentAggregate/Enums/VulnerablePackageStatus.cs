namespace ScanVul.Server.Domain.AgentAggregate.Enums;

/// <summary>
/// Statuses of vulnerable package
/// </summary>
public enum VulnerablePackageStatus
{
    /// <summary>
    /// Unknown status
    /// </summary>
    Unknown = 0,
    
    /// <summary>
    /// Vulnerable
    /// </summary>
    Vulnerable = 1,
    
    /// <summary>
    /// Scanner falsely marked package as vulnerable
    /// </summary>
    FalsePositive = 2,
    
    /// <summary>
    /// Package is vulnerable, but there is no patches currently
    /// </summary>
    Patchless = 3,
    
    /// <summary>
    /// Vulnerability of package is fixed alternatively
    /// </summary>
    Fixed = 4
}