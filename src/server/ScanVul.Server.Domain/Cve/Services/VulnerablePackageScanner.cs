using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;
using ScanVul.Server.Domain.Cve.Entities;
using ScanVul.Server.Domain.Cve.Enums;
using ScanVul.Server.Domain.Cve.Options;
using ScanVul.Server.Domain.Cve.Repositories;

namespace ScanVul.Server.Domain.Cve.Services;

public class VulnerablePackageScanner(
    ICveRepository cveRepository,
    IComputerRepository computerRepository,
    ILogger<VulnerablePackageScanner> logger,
    IOptions<ScanSettings> options,
    IUnitOfWork unitOfWork,
    VersionMatcher versionMatcher) : IVulnerablePackageScanner
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

    private async Task ScanInternalAsync(long computerId, CancellationToken ct)
    {
        var computer = await computerRepository.GetComputerWithAllPackagesAsync(computerId, ct);
        if (computer == null)
        {
            logger.LogError("Could not find computer {ComputerId}", computerId);
            throw new Exception($"Could not find computer {computerId}");
        }
        logger.LogInformation("Scanning packages of computer {ComputerId} for vulnerabilities", computerId);

        List<VulnerablePackage> vulnerablePackages = [];
        foreach (var package in computer.Packages)
        {
            vulnerablePackages.AddRange(await ScanPackageAsync(computer, package, ct));
        }

        var uniqueVulnerablePackages = vulnerablePackages
            .DistinctBy(x => (x.PackageInfoId, x.CveId))
            .ToList();
        
        var incomingIds = new HashSet<(long PackageInfoId, string CveId)>(uniqueVulnerablePackages
            .Select(x => (x.PackageInfoId, x.CveId)));
        var existingIds = new HashSet<(long PackageInfoId, string CveId)>(computer.VulnerablePackages
            .Select(x => (x.PackageInfoId, x.CveId)));
        
        // Remove not relevant vulnerable packages
        var toRemove = computer.VulnerablePackages
            .Where(x => !incomingIds.Contains((x.PackageInfoId, x.CveId)))
            .ToList();
        foreach (var item in toRemove) 
            computer.VulnerablePackages.Remove(item);
        
        // Add new ones
        var toAdd = uniqueVulnerablePackages
            .Where(x => !existingIds.Contains((x.PackageInfoId, x.CveId)))
            .ToList();
        computer.VulnerablePackages.AddRange(toAdd);

        await unitOfWork.SaveChangesAsync(ct);
        
        logger.LogInformation("Successfully scanned packages of computer {ComputerId} for vulnerabilities", computerId);
    }

    private async Task<List<VulnerablePackage>> ScanPackageAsync(Computer computer, PackageInfo package, CancellationToken ct = default)
    {
        var possibleCves = await cveRepository.GetMatchedCveDocumentsAsync(package, ct);

        List<VulnerablePackage> vulnerablePackages = [];
        
        // check CNA
        foreach (var cve in possibleCves)
        {
            foreach (var affectedItem in cve.Payload.Containers.Cna?.Affected ?? [])
            {
                if (!IsPackageAffectedItem(package.Name, affectedItem)) continue;
                
                foreach (var versionInfo in affectedItem.Versions)
                {
                    if (!IsPackageVersionAffected(package.Version, versionInfo)) continue;
                    
                    var vulnerablePackage = new VulnerablePackage(cve.Payload.CveMetadata.CveId, package, computer);
                    vulnerablePackages.Add(vulnerablePackage);
                }
            }
        }
        
        if (options.Value.AdpScan)
        {
            // check ADP (тут должна быть очень сложная логика, учитывающая ОС, пакетные менеджеры...)
            foreach (var cve in possibleCves)
            {
                foreach (var adp in cve.Payload.Containers.Adp)
                {
                    foreach (var affectedItem in adp.Affected)
                    {
                        if (!IsPackageAffectedItem(package.Name, affectedItem)) continue;

                        foreach (var versionInfo in affectedItem.Versions)
                        {
                            if (!IsPackageVersionAffected(package.Version, versionInfo)) continue;

                            var vulnerablePackage = new VulnerablePackage(cve.Payload.CveMetadata.CveId, package, computer);
                            vulnerablePackages.Add(vulnerablePackage);
                        }
                    }
                }
            }
        }

        return vulnerablePackages;
    }

    private static bool IsPackageAffectedItem(string packageName, AffectedItem affectedItem)
    {
        return affectedItem.Product.StartsWith(packageName, StringComparison.OrdinalIgnoreCase);
    }
    
    private bool IsPackageVersionAffected(string packageVersion, VersionInfo versionInfo)
    {
        try
        {
            if (versionInfo.Status != "affected")
                return false;
        
            if (!versionMatcher.TryCreateVersionObject(packageVersion, VersionMatchType.Unspecified, out var version))
                return false;
        
            if (versionInfo.LessThanOrEqual != null)
                return versionMatcher.Compare(
                    version, 
                    versionInfo.LessThanOrEqual, 
                    type: version.Type.ToVersionMatchType()) <= 0;

            if (versionInfo.LessThan != null)
                return versionMatcher.Compare(
                    version, 
                    versionInfo.LessThan,
                    type: version.Type.ToVersionMatchType()) < 0;

            if (!string.IsNullOrEmpty(versionInfo.Version) && versionInfo.Version != "0")
                return versionMatcher.Compare(
                    version, 
                    versionInfo.Version,
                    type: version.Type.ToVersionMatchType()) == 0;
        }
        catch (ArgumentException)
        {
            logger.LogDebug("Couldn't match versions: {PackageVersion} <=> {AffectedVersion}", packageVersion, versionInfo);
        }

        return false;
    }
}