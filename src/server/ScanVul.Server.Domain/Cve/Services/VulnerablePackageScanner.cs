using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;
using ScanVul.Server.Domain.Cve.Enums;
using ScanVul.Server.Domain.Cve.Options;
using ScanVul.Server.Domain.Cve.Repositories;
using ScanVul.Server.Domain.Cve.ValueObjects.Versions;

namespace ScanVul.Server.Domain.Cve.Services;

public class VulnerablePackageScanner(
    ISearchTermSanitizer sanitizer,
    ICveRepository cveRepository,
    IComputerRepository computerRepository,
    ILogger<VulnerablePackageScanner> logger,
    IOptions<ScanSettings> options,
    IUnitOfWork unitOfWork,
    IMemoryCache cache,
    VersionMatcher versionMatcher) : BaseVulnerablePackageScanner<VulnerablePackage>(unitOfWork, logger)
{
    protected override async Task<(
        Computer? Computer, 
        List<PackageInfo> Packages, 
        List<VulnerablePackage> VulnerablePackages
        )> GetComputerWithPackagesAsync(long computerId, CancellationToken ct)
    {
        var computer = await computerRepository.GetComputerWithCvePackagesAsync(computerId, ct);
        return (computer, computer?.Packages ?? [], computer?.VulnerablePackages ?? []);
    }

    protected override async Task<IReadOnlyCollection<VulnerablePackage>> ScanPackageAsync(
        Computer computer, 
        PackageInfo package, 
        CancellationToken ct = default)
    {
        var possibleCves = await cveRepository.GetMatchedCveVersionDocumentsAsync(package, ct);

        List<VulnerablePackage> vulnerablePackages = [];
        
        // check CNA
        foreach (var cve in possibleCves)
        {
            foreach (var affectedItem in cve.Payload.Containers?.Cna?.Affected ?? [])
            {
                if (!IsPackageAffectedItem(package, affectedItem.Product)) continue;
                
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
                foreach (var adp in cve.Payload.Containers?.Adp ?? [])
                {
                    foreach (var affectedItem in adp.Affected)
                    {
                        if (!IsPackageAffectedItem(package, affectedItem.Product)) continue;

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

    protected override Task SaveVulnerablePackagesChangesAsync(long computerId, List<VulnerablePackage> removedVulnerablePackages, List<VulnerablePackage> addedVulnerablePackages,
        CancellationToken ct = default)
    {
        cache.Set(CacheKeys.AddedVulnerablePackages(computerId), addedVulnerablePackages);
        cache.Set(CacheKeys.RemovedVulnerablePackages(computerId), removedVulnerablePackages);
        
        return Task.CompletedTask;
    }

    private bool IsPackageAffectedItem(PackageInfo computerPackage, string cvePackageName)
    {
        var sanitizePackageName = sanitizer.SanitizePackageName(computerPackage.Name).ToLowerInvariant();
        return cvePackageName.Trim().Contains(sanitizePackageName, StringComparison.InvariantCultureIgnoreCase);
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