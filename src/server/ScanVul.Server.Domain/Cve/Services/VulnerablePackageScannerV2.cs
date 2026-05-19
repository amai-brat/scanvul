using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;
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
                try
                {
                    if (!IsPackageAffectedItem(package, affectedItem.Product, computer)) continue;
                    if (!IsPackageVersionAffected(package.Version, affectedItem)) continue;
                }
                catch (Exception e)
                {
                    logger.LogInformation(e, "Error when scanning package {PackageId} {PackageName} {PackageVersion} with {AffectedItem}", 
                        package.Id, package.Name, package.Version, JsonSerializer.Serialize(affectedItem));
                }
                
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
                        try
                        {
                            if (!IsPackageAffectedItem(package, affectedItem.Product, computer)) continue;
                            if (!IsPackageVersionAffected(package.Version, affectedItem)) continue;
                        }
                        catch (Exception e)
                        {
                            logger.LogInformation(e, "Error when scanning package {PackageId} {PackageName} {PackageVersion} with {AffectedItem}", 
                                package.Id, package.Name, package.Version, JsonSerializer.Serialize(affectedItem));
                        }
                     
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
        versionMatcher.TryCreateVersionObject(packageVersionStr, VersionMatchType.Unspecified, out var defaultV);

        foreach (var entry in affectedItem.Versions)
        {
            // 1: Determine the correct version match type based on the CVE entry's schema
            var matchType = GetMatchTypeFromCve(entry.VersionType);

            if (!versionMatcher.TryCreateVersionObject(packageVersionStr, matchType, out var v))
            {
                v = defaultV;
                if (v == null) continue; // Unparsable target version
            }

            // Determine if this is an exact match or a range match
            var isRange = !string.IsNullOrEmpty(entry.LessThan) || !string.IsNullOrEmpty(entry.LessThanOrEqual);

            // 2: Evaluate Lower Bound (entry.Version)
            bool isLowerBoundMet;
            var isExactMatchMet = false;

            // "0" conventionally denotes the earliest possible version. "*" means arbitrarily small/large.
            if (string.IsNullOrEmpty(entry.Version) || entry.Version == "0" || entry.Version == "*")
            {
                isLowerBoundMet = true;
            }
            else if (versionMatcher.TryCreateVersionObject(entry.Version, matchType, out var lowerBound))
            {
                var comparison = versionMatcher.Compare(lowerBound, v); // lowerBound vs v
                isLowerBoundMet = comparison <= 0; // lowerBound <= v
                isExactMatchMet = comparison == 0; // lowerBound == v
            }
            else
            {
                continue; // Invalid version string in CVE, skip to next rule
            }

            // 3: Handle Exact Match Scenario (No LessThan and No LessThanOrEqual)
            if (!isRange)
            {
                if (isExactMatchMet)
                    return entry.Status.Equals("affected", StringComparison.OrdinalIgnoreCase);
                
                continue;
            }

            // 4: Handle Range Match Upper Bound
            if (!isLowerBoundMet)
                continue; // Not in range, skip to next rule

            var isUpperBoundMet = false;

            // Check lessThan (exclusive)
            if (!string.IsNullOrEmpty(entry.LessThan) && entry.LessThan != "*")
            {
                if (versionMatcher.TryCreateVersionObject(entry.LessThan, matchType, out var upperBound))
                {
                    isUpperBoundMet = versionMatcher.Compare(v, upperBound) < 0; // v < lessThan
                }
            }
            // Check lessThanOrEqual (inclusive)
            else if (!string.IsNullOrEmpty(entry.LessThanOrEqual) && entry.LessThanOrEqual != "*")
            {
                if (versionMatcher.TryCreateVersionObject(entry.LessThanOrEqual, matchType, out var upperBound))
                {
                    isUpperBoundMet = versionMatcher.Compare(v, upperBound) <= 0; // v <= lessThanOrEqual
                }
            }
            else if (entry.LessThan == "*" || entry.LessThanOrEqual == "*")
            {
                // "*" indicates an arbitrarily large number
                isUpperBoundMet = true; 
            }

            // 5: Evaluate Status and Status Changes if within bounds
            if (isLowerBoundMet && isUpperBoundMet)
            {
                var status = entry.Status;

                if (entry.Changes is { Count: > 0 })
                {
                    // Parse changes into valid version objects so we can sort them correctly
                    var parsedChanges = entry.Changes
                        .Select(c => new 
                        { 
                            Change = c, 
                            Success = versionMatcher.TryCreateVersionObject(c.At, matchType, out var parsedAt),
                            ParsedAt = parsedAt 
                        })
                        .Where(c => c.Success)
                        .ToList();

                    parsedChanges.Sort((a, b) => versionMatcher.Compare(a.ParsedAt!, b.ParsedAt!));

                    foreach (var changeData in parsedChanges)
                    {
                        if (versionMatcher.Compare(changeData.ParsedAt!, v) <= 0) // change.at <= v
                        {
                            status = changeData.Change.Status;
                        }
                    }
                }

                // Return true if the final status in this matching range is "affected"
                return status.Equals("affected", StringComparison.OrdinalIgnoreCase);
            }
        }

        // 6: Fallback: return product.defaultStatus ("unknown" evaluates to false)
        return (affectedItem.DefaultStatus ?? "unknown").Equals("affected", StringComparison.OrdinalIgnoreCase);
    }

    private static VersionMatchType GetMatchTypeFromCve(string? cveVersionType)
    {
        if (string.IsNullOrWhiteSpace(cveVersionType))
            return VersionMatchType.Unspecified;

        return cveVersionType.ToLowerInvariant() switch
        {
            "semver" => VersionMatchType.SemVer,
            "python" => VersionMatchType.Pep440,
            "maven" => VersionMatchType.Base,
            "rpm" => VersionMatchType.Rpm,
            "dpkg" => VersionMatchType.Dpkg,
            "apk" => VersionMatchType.Apk,
            "custom" => VersionMatchType.Unspecified,
            "pacman" => VersionMatchType.Pacman,
            _ => VersionMatchType.Unspecified
        };
    }
}