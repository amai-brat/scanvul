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

public class VulnerablePackageScannerV2(
    ISearchTermSanitizer sanitizer,
    ICveRepository cveRepository,
    IComputerRepository computerRepository,
    ILogger<VulnerablePackageScanner> logger,
    IOptionsMonitor<ScanSettings> options,
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
                if (!IsPackageAffectedItem(package, affectedItem.Product, computer)) continue;
                if (!IsPackageVersionAffected(package.Version, affectedItem)) continue;
                     
                var vulnerablePackage = new VulnerablePackage(cve.Payload.CveMetadata.CveId, package, computer);
                vulnerablePackages.Add(vulnerablePackage);
            }
        }
        
        if (options.CurrentValue.AdpScan)
        {
            // check ADP (тут должна быть очень сложная логика, учитывающая ОС, пакетные менеджеры...)
            foreach (var cve in possibleCves)
            {
                foreach (var adp in cve.Payload.Containers?.Adp ?? [])
                {
                    foreach (var affectedItem in adp.Affected)
                    {
                        if (!IsPackageAffectedItem(package, affectedItem.Product, computer)) continue;
                        if (!IsPackageVersionAffected(package.Version, affectedItem)) continue;
                     
                        var vulnerablePackage = new VulnerablePackage(cve.Payload.CveMetadata.CveId, package, computer);
                        vulnerablePackages.Add(vulnerablePackage);
                    }
                }
            }
        }

        return vulnerablePackages
            .DistinctBy(x => (x.VulnerabilityId, x.PackageInfoId, x.ComputerId))
            .ToList();
    }

    protected override Task SaveVulnerablePackagesChangesAsync(long computerId, List<VulnerablePackage> removedVulnerablePackages, List<VulnerablePackage> addedVulnerablePackages,
        CancellationToken ct = default)
    {
        cache.Set(CacheKeys.AddedVulnerablePackages(computerId), addedVulnerablePackages);
        cache.Set(CacheKeys.RemovedVulnerablePackages(computerId), removedVulnerablePackages);
        
        return Task.CompletedTask;
    }

    protected override async Task PostScanAsync(long computerId, CancellationToken ct)
    {
        if (options.CurrentValue.DumpVersionCreationRecords)
        {
            var filename = $"versions_comp{computerId}_{DateTime.UtcNow:O}.json";
            await versionMatcher.DumpVersionCreationRecordsAsync(filename, ct);
        }
    }

    private bool IsPackageAffectedItem(PackageInfo computerPackage, string cveProduct, Computer computer)
    {
        var sanitizedPackageName = sanitizer.SanitizePackageName(computerPackage.Name).ToLowerInvariant().Trim();
        var product = cveProduct.ToLowerInvariant().Trim();
        
        if (SystemsWithRelaxedPackageNames.Contains(computer.OperatingSystem))
        {
            return product.Contains(sanitizedPackageName, StringComparison.OrdinalIgnoreCase) ||
                   sanitizedPackageName.Contains(product, StringComparison.OrdinalIgnoreCase);
        }
        
        var maxDistance = options.CurrentValue.MaxLevenshteinDistance; 
        if (Math.Abs(sanitizedPackageName.Length - product.Length) > maxDistance)
        {
            return false;
        }
        
        var distance = CalculateBoundedLevenshteinDistance(sanitizedPackageName.AsSpan(), product.AsSpan(), maxDistance);
        return distance <= maxDistance;
    }
    
    private static int CalculateBoundedLevenshteinDistance(
        ReadOnlySpan<char> source,
        ReadOnlySpan<char> target,
        int maxDistance)
    {
        if (source.Length == 0) return target.Length;
        if (target.Length == 0) return source.Length;
        
        if (source.Length > target.Length)
        {
            var temp = source;
            source = target;
            target = temp;
        }

        var length = source.Length;
        
        var distances = length <= 256 
            ? stackalloc int[length + 1] 
            : new int[length + 1];
        
        for (var i = 0; i <= length; i++)
        {
            distances[i] = i;
        }

        for (var i = 0; i < target.Length; i++)
        {
            var previousDiagonal = distances[0];
            distances[0] = i + 1;
            
            var currentMinDistance = distances[0];

            for (var j = 0; j < length; j++)
            {
                var previousDiagonalSave = distances[j + 1];

                if (target[i] == source[j])
                {
                    distances[j + 1] = previousDiagonal;
                }
                else
                {
                    distances[j + 1] = Math.Min(
                        Math.Min(
                            distances[j],       // Insertion
                            distances[j + 1]    // Deletion
                        ),
                        previousDiagonal        // Substitution
                    ) + 1;
                }

                if (distances[j + 1] < currentMinDistance)
                {
                    currentMinDistance = distances[j + 1];
                }

                previousDiagonal = previousDiagonalSave;
            }

            if (currentMinDistance > maxDistance)
            {
                return maxDistance + 1; 
            }
        }

        return distances[length];
    }
    
    private bool IsPackageVersionAffected(string packageVersionStr, AffectedItem affectedItem)
    {
        if (!versionMatcher.TryCreateVersionObject(packageVersionStr, VersionMatchType.Unspecified, out var v))
            return false;
        
        foreach (var entry in affectedItem.Versions)
        {
            // Setup lower bound (entry.version). "0" conventionally denotes earliest possible version.
            // If entry.Version is "unspecified" or "*", we treat it as matching the lowest possible.
            var isLowerBoundMet = true; 
            if (!string.IsNullOrEmpty(entry.Version) && entry.Version != "0" && entry.Version != "*")
            {
                if (versionMatcher.TryCreateVersionObject(entry.Version, v.Type.ToVersionMatchType(), out var lowerBound))
                {
                    isLowerBoundMet = versionMatcher.Compare(lowerBound, v) <= 0; // lowerBound <= v
                }
            }

            // 1: Exact Version Match
            if (string.IsNullOrEmpty(entry.LessThan) && string.IsNullOrEmpty(entry.LessThanOrEqual))
            {
                if (versionMatcher.TryCreateVersionObject(entry.Version, v.Type.ToVersionMatchType(), out var exactVer))
                {
                    if (versionMatcher.Compare(v, exactVer) == 0)
                        return entry.Status.Equals("affected", StringComparison.OrdinalIgnoreCase);
                }
                continue;
            }

            // 2: Range Match
            var isUpperBoundMet = false;
            
            // Check lessThan (exclusive)
            if (!string.IsNullOrEmpty(entry.LessThan) && entry.LessThan != "*")
            {
                if (versionMatcher.TryCreateVersionObject(entry.LessThan, v.Type.ToVersionMatchType(), out var upperBound))
                {
                    isUpperBoundMet = versionMatcher.Compare(v, upperBound) < 0; // v < lessThan
                }
            }
            // Check lessThanOrEqual (inclusive)
            else if (!string.IsNullOrEmpty(entry.LessThanOrEqual) && entry.LessThanOrEqual != "*")
            {
                if (versionMatcher.TryCreateVersionObject(entry.LessThanOrEqual, v.Type.ToVersionMatchType(), out var upperBound))
                {
                    isUpperBoundMet = versionMatcher.Compare(v, upperBound) <= 0; // v <= lessThanOrEqual
                }
            }
            else if (entry.LessThan == "*" || entry.LessThanOrEqual == "*")
            {
                // "*" indicates an arbitrarily large number
                isUpperBoundMet = true; 
            }

            // If version is within the defined range
            if (isLowerBoundMet && isUpperBoundMet)
            {
                var status = entry.Status;

                if (entry.Changes == null) 
                    return status.Equals("affected", StringComparison.OrdinalIgnoreCase);
                
                var sortedChanges = entry.Changes.OrderBy(c => c.At).ToList(); 
                foreach (var change in sortedChanges)
                {
                    if (versionMatcher.TryCreateVersionObject(change.At, v.Type.ToVersionMatchType(), out var changeVer))
                    {
                        if (versionMatcher.Compare(changeVer, v) <= 0) // change.at <= v
                        {
                            status = change.Status;
                        }
                    }
                }

                return status.Equals("affected", StringComparison.OrdinalIgnoreCase);
            }
        }

        // Fallback: return product.defaultStatus
        return (affectedItem.DefaultStatus ?? "unknown").Equals("affected", StringComparison.OrdinalIgnoreCase);
    }
}