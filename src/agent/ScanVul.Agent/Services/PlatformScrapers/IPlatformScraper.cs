using ScanVul.Agent.Models;

namespace ScanVul.Agent.Services.PlatformScrapers;

public interface IPlatformScraper
{
    Task<List<PackageInfo>> ScrapePackagesAsync(CancellationToken ct = default);
    Task<ComputerInfo> ScrapeComputerInfoAsync(CancellationToken ct = default);
}