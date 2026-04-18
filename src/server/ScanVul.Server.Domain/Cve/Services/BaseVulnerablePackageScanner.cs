using Microsoft.Extensions.Logging;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Domain.Cve.Services;

public abstract class BaseVulnerablePackageScanner<TVulnPkg>(
    IUnitOfWork unitOfWork,
    ILogger logger) : IVulnerablePackageScanner
    where TVulnPkg : BaseVulnerablePackage
{
    public async Task ScanAsync(long computerId, CancellationToken ct = default)
    {
        try
        {
            await ScanInternalAsync(computerId, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error scanning vulnerable package of computer {ComputerId}", computerId);
        }
    }
    
    protected abstract Task<(
        Computer? Computer, 
        List<PackageInfo> Packages,
        List<TVulnPkg> VulnerablePackages
        )> GetComputerWithPackagesAsync(long computerId, CancellationToken ct);

    protected abstract Task<IReadOnlyCollection<TVulnPkg>> ScanPackageAsync(
        Computer computer,
        PackageInfo package,
        CancellationToken ct = default);
    
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
        
        var incomingIds = new HashSet<(long PackageInfoId, string CveId)>(uniqueVulnerablePackages
            .Select(x => (x.PackageInfoId, x.VulnerabilityId)));
        var existingIds = new HashSet<(long PackageInfoId, string CveId)>(computerWithPackages.VulnerablePackages
            .Select(x => (x.PackageInfoId, x.VulnerabilityId)));
        
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
        computerWithPackages.VulnerablePackages.AddRange(toAdd);

        await unitOfWork.SaveChangesAsync(ct);
        
        logger.LogInformation("Successfully scanned packages of computer {ComputerId} for vulnerabilities", computerId);
    }
}