using Microsoft.Extensions.Logging;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.AgentAggregate.Enums;
using ScanVul.Server.Domain.AgentAggregate.Services;
using ScanVul.Server.Domain.Common;
using OperatingSystem = ScanVul.Server.Domain.AgentAggregate.Enums.OperatingSystem;

namespace ScanVul.Server.Domain.Cve.Services;

public abstract class BaseVulnerablePackageScanner<TVulnPkg>(
    IUnitOfWork unitOfWork,
    ILogger logger) : IVulnerablePackageScanner
    where TVulnPkg : BaseVulnerablePackage
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly HashSet<VulnerablePackageStatus> RollingStatusesAfterUpdate = [
        VulnerablePackageStatus.Vulnerable,
        VulnerablePackageStatus.FalsePositive,
        VulnerablePackageStatus.Patchless,
        VulnerablePackageStatus.Fixed,
    ];

    /// <summary>
    /// OSes with relaxed (not strict) package names (e.g. in Linux distributions package managers have strict package names, but Uninstall registers on Windows - not)
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    protected static readonly IReadOnlySet<OperatingSystem> SystemsWithRelaxedPackageNames = new HashSet<OperatingSystem>
    {
        OperatingSystem.Windows,
    };
    
    public async Task ScanAsync(long computerId, CancellationToken ct = default)
    {
        try
        {
            await ScanInternalAsync(computerId, ct);
            
            await PostScanAsync(computerId, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error scanning vulnerable package of computer {ComputerId}", computerId);
        }
    }
    
    /// <summary>
    /// Get computer, its packages and vulnerable packages
    /// </summary>
    /// <param name="computerId">Computer ID</param>
    /// <param name="ct">Cancellation token</param>
    protected abstract Task<(
        Computer? Computer, 
        List<PackageInfo> Packages,
        List<TVulnPkg> VulnerablePackages
        )> GetComputerWithPackagesAsync(long computerId, CancellationToken ct);

    protected abstract Task<IReadOnlyCollection<TVulnPkg>> ScanPackageAsync(
        Computer computer,
        PackageInfo package,
        CancellationToken ct = default);

    /// <summary>
    /// Save changes of vulnerable packages (in cache) to use them later in <see cref="IScanSnapshotGenerator"/> 
    /// </summary>
    /// <param name="computerId">Computer ID</param>
    /// <param name="removedVulnerablePackages">Removed ones</param>
    /// <param name="addedVulnerablePackages">Added ones</param>
    /// <param name="ct">Cancellation token</param>
    protected abstract Task SaveVulnerablePackagesChangesAsync(
        long computerId,
        List<TVulnPkg> removedVulnerablePackages,
        List<TVulnPkg> addedVulnerablePackages, 
        CancellationToken ct = default);

    /// <summary>
    /// Things to do after scan
    /// </summary>
    /// <param name="computerId">Computer ID</param>
    /// <param name="ct">Cancellation token</param>
    protected virtual Task PostScanAsync(long computerId, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Get not rolling packages (rolling package means package that was existed on computer and updated) 
    /// </summary>
    /// <param name="removedVulnerablePackages">Removed vulnerable packages</param>
    /// <param name="addedVulnerablePackages">Added vulnerable packages</param>
    /// <param name="existingPackageStatuses">Dictionary with existing vulnerable packages: (name, vuln_id) => status</param>
    /// <returns>Tuple with not rolling packages</returns>
    private static (
        List<TVulnPkg> NotRollingRemoved,
        List<TVulnPkg> NotRollingAdded
        ) GetNotRollingPackages(
            List<TVulnPkg> removedVulnerablePackages,
            List<TVulnPkg> addedVulnerablePackages, 
            Dictionary<(string PackageName, string VulnerabilityId), VulnerablePackageStatus> existingPackageStatuses)
    {
        List<TVulnPkg> notRollingRemoved = [];
        foreach (var vulnPkg in removedVulnerablePackages)
        {
            // exists => rolling
            if (existingPackageStatuses.TryGetValue((vulnPkg.PackageInfo.Name, vulnPkg.VulnerabilityId), out _)) 
                continue;
            
            notRollingRemoved.Add(vulnPkg);
        }
        
        List<TVulnPkg> notRollingAdded = [];
        foreach (var vulnPkg in addedVulnerablePackages)
        {
            // exists => rolling
            if (existingPackageStatuses.TryGetValue((vulnPkg.PackageInfo.Name, vulnPkg.VulnerabilityId), out _)) 
                continue;
            
            notRollingAdded.Add(vulnPkg);
        }
        
        return (notRollingRemoved, notRollingAdded);
    }
    
    
    private async Task ScanInternalAsync(long computerId, CancellationToken ct)
    {
        var computerWithPackages = await GetComputerWithPackagesAsync(computerId, ct);
        if (computerWithPackages.Computer == null)
        {
            logger.LogError("Could not find computer {ComputerId}", computerId);
            throw new Exception($"Could not find computer {computerId}");
        }
        logger.LogInformation("Scanning packages of computer {ComputerId} for vulnerabilities", computerId);

        List<TVulnPkg> vulnerablePackages = [];
        foreach (var package in computerWithPackages.Packages)
        {
            vulnerablePackages.AddRange(await ScanPackageAsync(computerWithPackages.Computer, package, ct));
        }

        var uniqueVulnerablePackages = vulnerablePackages
            .DistinctBy(x => (x.PackageInfoId, x.VulnerabilityId))
            .ToList();
        
        var incomingIds = new HashSet<(long PackageInfoId, string VulnerabilityId)>(uniqueVulnerablePackages
            .Select(x => (x.PackageInfoId, x.VulnerabilityId)));
        var existingIds = new HashSet<(long PackageInfoId, string VulnerabilityId)>(computerWithPackages.VulnerablePackages
            .Select(x => (x.PackageInfoId, x.VulnerabilityId)));
        var existingPackageStatuses = computerWithPackages.VulnerablePackages
            .ToDictionary(x => (x.PackageInfo.Name, x.VulnerabilityId), x => x.Status); 
        
        // Remove not relevant vulnerable packages
        var toRemove = computerWithPackages.VulnerablePackages
            .Where(x => !incomingIds.Contains((x.PackageInfoId, x.VulnerabilityId)))
            .ToList();
        foreach (var item in toRemove) 
            computerWithPackages.VulnerablePackages.Remove(item);
        
        // Add new ones
        var toAdd = uniqueVulnerablePackages
            .Where(x => !existingIds.Contains((x.PackageInfoId, x.VulnerabilityId)))
            .ToList();
        
        foreach (var vulnPkg in toAdd)
        {
            // if package version changed (updated), pass vulnerable package status
            if (!existingPackageStatuses.TryGetValue((vulnPkg.PackageInfo.Name, vulnPkg.VulnerabilityId), 
                    out var currentStatus)) continue;
            if (RollingStatusesAfterUpdate.Contains(currentStatus))
            {
                vulnPkg.Status = currentStatus;
            }
        }
        
        computerWithPackages.VulnerablePackages.AddRange(toAdd);

        await unitOfWork.SaveChangesAsync(ct);

        var notRolling = GetNotRollingPackages(toRemove, toAdd, existingPackageStatuses);
        await SaveVulnerablePackagesChangesAsync(computerId, notRolling.NotRollingRemoved, notRolling.NotRollingAdded, ct);
        
        logger.LogInformation("Successfully scanned packages of computer {ComputerId} for vulnerabilities. " +
                              "Found {VulnerablePackagesCount} vulnerable packages", 
            computerId, computerWithPackages.VulnerablePackages.Count);
    }
}