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
        var computer = await computerRepository.GetComputerWithAllPackagesAsync(computerId, ct);
        if (computer == null)
        {
            logger.LogError("Could not find computer {ComputerId}", computerId);
            throw new Exception($"Could not find computer {computerId}");
        }
                
        foreach (var package in computer.Packages)
        {
            await ScanPackageAsync(computer, package, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task ScanPackageAsync(Computer computer, PackageInfo package, CancellationToken ct = default)
    {
        var possibleCves = await cveRepository.GetMatchedCveDocumentsAsync(package, ct);

        List<VulnerablePackage> vulnerablePackages = [];
        
        // check CNA
        foreach (var cve in possibleCves)
        {
            foreach (var affectedItem in cve.Payload.Containers.Cna?.Affected ?? [])
            {
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
                        foreach (var versionInfo in affectedItem.Versions)
                        {
                            if (!IsPackageVersionAffected(package.Version, versionInfo)) continue;

                            var vulnerablePackage =
                                new VulnerablePackage(cve.Payload.CveMetadata.CveId, package, computer);
                            vulnerablePackages.Add(vulnerablePackage);
                        }
                    }
                }
            }
        }

        computer.VulnerablePackages = vulnerablePackages
            .DistinctBy(x => x.CveId)
            .ToList();
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
            logger.LogWarning("Couldn't match versions: {PackageVersion} <=> {AffectedVersion}", packageVersion, versionInfo);
        }

        return false;
    }
}