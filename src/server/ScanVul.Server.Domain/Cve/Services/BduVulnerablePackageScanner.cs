using Microsoft.Extensions.Logging;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;
using ScanVul.Server.Domain.Cve.Repositories;

namespace ScanVul.Server.Domain.Cve.Services;

public class BduVulnerablePackageScanner(
    IBduRepository bduRepository,
    IComputerRepository computerRepository,
    ILogger<BduVulnerablePackageScanner> logger,
    IUnitOfWork unitOfWork) : IVulnerablePackageScanner
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
            .DistinctBy(x => (x.PackageInfoId, x.BduId))
            .ToList();
        
        var incomingIds = new HashSet<(long PackageInfoId, string BduId)>(uniqueVulnerablePackages
            .Select(x => (x.PackageInfoId, x.BduId)));
        var existingIds = new HashSet<(long PackageInfoId, string BduId)>(computer.BduVulnerablePackages
            .Select(x => (x.PackageInfoId, x.BduId)));
        
        // Remove not relevant vulnerable packages
        var toRemove = computer.BduVulnerablePackages
            .Where(x => !incomingIds.Contains((x.PackageInfoId, x.BduId)))
            .ToList();
        foreach (var item in toRemove) 
            computer.BduVulnerablePackages.Remove(item);
        
        // Add new ones
        var toAdd = uniqueVulnerablePackages
            .Where(x => !existingIds.Contains((x.PackageInfoId, x.BduId)))
            .ToList();
        computer.BduVulnerablePackages.AddRange(toAdd);

        await unitOfWork.SaveChangesAsync(ct);
        
        logger.LogInformation("Successfully scanned packages of computer {ComputerId} for vulnerabilities (BDU). Found: {Count}", computerId, computer.BduVulnerablePackages.Count);
    }

    private async Task<List<BduVulnerablePackage>> ScanPackageAsync(Computer computer, PackageInfo package, CancellationToken ct = default)
    {
        var possibleBduDocuments = await bduRepository.GetMatchedBduVersionDocumentsAsync(package, ct);

        // saves all matched by package name,
        // to match by version i need to change 'version' field in bdu-index like in cve-index
        List<BduVulnerablePackage> vulnerablePackages = [];
        foreach (var bdu in possibleBduDocuments)
        {
            var vulnerablePackage = new BduVulnerablePackage(bdu.Identifier.First(), package, computer);
            vulnerablePackages.Add(vulnerablePackage);
        }

        return vulnerablePackages;
    }
}