namespace ScanVul.Server.Domain.Common;

public static class CacheKeys
{
    public static string AddedPackages(long computerId) => $"comp:{computerId}+pkgs";
    public static string RemovedPackages(long computerId) => $"comp:{computerId}-pkgs";
    
    public static string AddedVulnerablePackages(long computerId) => $"comp:{computerId}+vulns";
    public static string RemovedVulnerablePackages(long computerId) => $"comp:{computerId}-vulns";
    
    public static string AddedBduVulnerablePackages(long computerId) => $"comp:{computerId}+bdu_vulns";
    public static string RemovedBduVulnerablePackages(long computerId) => $"comp:{computerId}-bdu_vulns";
}