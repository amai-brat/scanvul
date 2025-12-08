using Microsoft.Extensions.Logging;
using ScanVul.Server.Domain.Entities;
using ScanVul.Server.Domain.Repositories;

namespace ScanVul.Server.Domain.Services;

public class VulnerablePackageScanner(
    ICveRepository cveRepository,
    IComputerRepository computerRepository,
    ILogger<VulnerablePackageScanner> logger,
    IUnitOfWork unitOfWork) : IVulnerablePackageScanner
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
    }

    private async Task ScanPackageAsync(Computer computer, PackageInfo package, CancellationToken ct = default)
    {
        var possibleCves = await cveRepository.GetMatchedCveDocumentsAsync(package, ct);
        
        // TODO
    }
}