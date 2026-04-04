namespace ScanVul.Server.Domain.Reports.Models;

/// <summary>
/// Model for severity level (CVSS ranges) counts
/// </summary>
/// <param name="CriticalCount">CVSS: [9.0; 10.0]</param>
/// <param name="HighCount">CVSS: [7.0; 9.0)</param>
/// <param name="MediumCount">CVSS: [4.0; 7.0)</param>
/// <param name="LowCount">CVSS: [0.0; 4.0)</param>
public record SeverityStatsModel(
    int CriticalCount, 
    int HighCount, 
    int MediumCount, 
    int LowCount);