namespace ScanVul.Server.Domain.Reports.Models;

/// <summary>
/// Model for agent entry
/// </summary>
/// <param name="Id">Agent ID</param>
/// <param name="Name">Computer name</param>
/// <param name="IpAddress">IP address of computer</param>
/// <param name="OperatingSystem">Operating system</param>
/// <param name="PackagesCount">Packages count</param>
/// <param name="CveSeverityStats">CVE severity stats for agent</param>
/// <param name="BduSeverityStats">BDU severity stats for agent</param>
public record AgentInfoModel(
    long Id,
    string Name, 
    string IpAddress, 
    string OperatingSystem, 
    int PackagesCount, 
    SeverityStatsModel CveSeverityStats, 
    SeverityStatsModel BduSeverityStats);