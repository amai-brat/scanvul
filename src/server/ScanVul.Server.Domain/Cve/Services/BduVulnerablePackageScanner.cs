using Microsoft.Extensions.Logging;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;
using ScanVul.Server.Domain.Cve.Enums;
using ScanVul.Server.Domain.Cve.Repositories;
using ScanVul.Server.Domain.Cve.ValueObjects.Versions;

namespace ScanVul.Server.Domain.Cve.Services;

public class BduVulnerablePackageScanner(
    IBduRepository bduRepository,
    IComputerRepository computerRepository,
    ILogger<BduVulnerablePackageScanner> logger,
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
            logger.LogError(ex, "Error scanning vulnerable package of computer {ComputerId} (BDU)", computerId);
        }
    }

    private async Task ScanInternalAsync(long computerId, CancellationToken ct)
    {
        var computer = await computerRepository.GetComputerWithBduPackagesAsync(computerId, ct);
        if (computer == null)
        {
            logger.LogError("Could not find computer {ComputerId}", computerId);
            throw new Exception($"Could not find computer {computerId}");
        }
        logger.LogInformation("Scanning packages of computer {ComputerId} for vulnerabilities (BDU)", computerId);

        List<BduVulnerablePackage> vulnerablePackages = [];
        foreach (var package in computer.Packages)
        {
            vulnerablePackages.AddRange(await ScanPackageAsync(computer, package, ct));
        }

        var uniqueVulnerablePackages = vulnerablePackages
            .DistinctBy(x => (x.PackageInfoId, BduId: x.VulnerabilityId))
            .ToList();
        
        var incomingIds = new HashSet<(long PackageInfoId, string BduId)>(uniqueVulnerablePackages
            .Select(x => (x.PackageInfoId, BduId: x.VulnerabilityId)));
        var existingIds = new HashSet<(long PackageInfoId, string BduId)>(computer.BduVulnerablePackages
            .Select(x => (x.PackageInfoId, BduId: x.VulnerabilityId)));
        
        // Remove not relevant vulnerable packages
        var toRemove = computer.BduVulnerablePackages
            .Where(x => !incomingIds.Contains((x.PackageInfoId, x.VulnerabilityId)))
            .ToList();
        foreach (var item in toRemove) 
            computer.BduVulnerablePackages.Remove(item);
        
        // Add new ones
        var toAdd = uniqueVulnerablePackages
            .Where(x => !existingIds.Contains((x.PackageInfoId, x.VulnerabilityId)))
            .ToList();
        computer.BduVulnerablePackages.AddRange(toAdd);

        await unitOfWork.SaveChangesAsync(ct);
        
        logger.LogInformation("Successfully scanned packages of computer {ComputerId} for vulnerabilities (BDU). Found: {Count}", computerId, computer.BduVulnerablePackages.Count);
    }

    private async Task<List<BduVulnerablePackage>> ScanPackageAsync(Computer computer, PackageInfo package, CancellationToken ct = default)
    {
        var possibleBduDocuments = await bduRepository.GetMatchedBduVersionDocumentsAsync(package, ct);

        List<BduVulnerablePackage> vulnerablePackages = [];
        foreach (var bdu in possibleBduDocuments)
        {
            foreach (var bduSoft in bdu.VulnerableSoftware!.Soft)
            {
                if (!IsPackageNameAffected(package, bduSoft.Name)) continue;
                if (!IsPackageVersionAffected(package.Version, bduSoft)) continue;
            
                var vulnerablePackage = new BduVulnerablePackage(bdu.Identifier.First(), package, computer);
                vulnerablePackages.Add(vulnerablePackage);
            }
        }

        return vulnerablePackages;
    }
    
    private static bool IsPackageNameAffected(PackageInfo computerPackage, string bduPackageName)
    {
        var sanitizePackageName = SearchTermSanitizer.SanitizePackageName(computerPackage.Name).ToLowerInvariant();
        return bduPackageName.Trim().Contains(sanitizePackageName, StringComparison.InvariantCultureIgnoreCase);
    }
    
    /// <summary>
    /// Check if package version is affected
    /// </summary>
    /// <remarks>Returns true also if couldn't check. Admin should check himself, mark as false-positive if needed</remarks>
    /// <param name="packageVersion">Version of package to check</param>
    /// <param name="soft">BduSoft document from bdu-index</param>
    /// <returns></returns>
    private bool IsPackageVersionAffected(string packageVersion, BduSoft soft)
    {
        try
        {
            if (soft.VersionInfo?.Version is not "<ok>")
                return IsPackageVersionAffectedIfVersionIsNotOk(packageVersion, soft.VersionInfo?.Version);
            
            if (!versionMatcher.TryCreateVersionObject(packageVersion, VersionMatchType.Base, out var version))
                return true;

            var versionInfo = soft.VersionInfo;

            bool? lessThanOrEqual = null;
            if (versionInfo.LessThanOrEqual != null)
            {
                lessThanOrEqual = versionMatcher.Compare(version, versionInfo.LessThanOrEqual, 
                    type: version.Type.ToVersionMatchType()) <= 0;
            }
            
            bool? lessThan = null;
            if (versionInfo.LessThan != null)
            {
                lessThan = versionMatcher.Compare(version, versionInfo.LessThan,
                    type: version.Type.ToVersionMatchType()) < 0;
            }
            
            bool? greaterThanOrEqual = null;
            if (versionInfo.GreaterThanOrEqual != null)
            {
                greaterThanOrEqual = versionMatcher.Compare(version, versionInfo.GreaterThanOrEqual, 
                    type: version.Type.ToVersionMatchType()) >= 0;
            }
            
            if (lessThanOrEqual is not null && greaterThanOrEqual is not null)
                return lessThanOrEqual.Value && greaterThanOrEqual.Value;
                
            if (lessThan is not null && greaterThanOrEqual is not null)
                return lessThan.Value && greaterThanOrEqual.Value;
            
            if (lessThanOrEqual is not null)
                return lessThanOrEqual.Value;
            
            if (lessThan is not null)
                return lessThan.Value;

            if (greaterThanOrEqual is not null)
                return greaterThanOrEqual.Value;
        }
        catch (ArgumentException)
        {
            logger.LogDebug("Couldn't match versions: {PackageVersion} <=> {AffectedVersion}", packageVersion, soft.VersionInfo);
        }

        return true;
    }

    /// <summary>
    /// Check whether package version is affected if BduSnapshotDownloadWorker couldn't parse version.
    /// Check only by BaseNumberVersion, if couldn't returns true
    /// </summary>
    /// <param name="packageVersion">Version of package to check</param>
    /// <param name="bduVersion">BduSoft version</param>
    /// <returns></returns>
    private bool IsPackageVersionAffectedIfVersionIsNotOk(string? packageVersion, string? bduVersion)
    {
        if (string.IsNullOrWhiteSpace(packageVersion) || string.IsNullOrWhiteSpace(bduVersion))
            return true;
        
        if (!versionMatcher.TryCreateVersionObject(packageVersion, VersionMatchType.BaseNumber, out var packageVersionObj))
            return true;
        
        if (!versionMatcher.TryCreateVersionObject(bduVersion, VersionMatchType.BaseNumber, out var bduVersionObj))
            return true;
        
        return packageVersionObj.CompareTo(bduVersionObj) == 0;
    }
}