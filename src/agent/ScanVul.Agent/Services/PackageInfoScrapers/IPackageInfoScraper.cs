using ScanVul.Agent.Models;

namespace ScanVul.Agent.Services.PackageInfoScrapers;

public interface IPackageInfoScraper
{
    Task<List<PackageInfo>> ScrapeAsync(CancellationToken ct = default);
}