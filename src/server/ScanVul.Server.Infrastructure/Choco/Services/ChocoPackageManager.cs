using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using ScanVul.Server.Domain.PackageManagers.Services;
using ScanVul.Server.Domain.PackageManagers.ValueObjects;

namespace ScanVul.Server.Infrastructure.Choco.Services;

public class ChocoPackageManager : IPackageManager
{
    private const string ChocolateyFeedUrl = "https://community.chocolatey.org/api/v2/";
    
    public async Task<List<PackageMetadata>> SearchAsync(string searchTerm, CancellationToken ct = default)
    {
        var logger = NullLogger.Instance;
        
        var source = new PackageSource(ChocolateyFeedUrl)
        {
            AllowInsecureConnections = true
        };
        var repository = Repository.Factory.GetCoreV3(source);

        var searchResource = await repository.GetResourceAsync<PackageSearchResource>(ct);
        var searchResults = await searchResource.SearchAsync(
                searchTerm,
                new SearchFilter(includePrerelease: false),
                skip: 0,
                take: 5, 
                logger, 
                ct);

        var tasks = searchResults
            .Select(async x =>
            {
                var versions = await x.GetVersionsAsync();

                return new PackageMetadata(
                    Name: x.Title,
                    Url: x.PackageDetailsUrl.ToString(),
                    LastVersion: x.Identity.Version?.OriginalVersion
                                 ?? x.Identity.Version?.Version.ToString()
                                 ?? "<unknown>",
                    Summary: x.Summary,
                    Versions: versions
                        .Select(v => v.Version.Version.ToString())
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToList());
            });

        var arr = await Task.WhenAll(tasks);
        return arr.ToList();
    }
}