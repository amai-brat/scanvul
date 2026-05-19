using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using ScanVul.Server.Domain.PackageManagers.Services;
using ScanVul.Server.Domain.PackageManagers.ValueObjects;
using ScanVul.Server.Infrastructure.Data;

namespace ScanVul.Server.Infrastructure.Choco.Services;

public class WingetPackageManager(AppDbContext dbContext) : IPackageManager
{
    public async Task<List<PackageMetadata>> SearchAsync(string searchTerm, CancellationToken ct = default)
    {
        var packages = await dbContext.WingetPackages
            .Where(p => 
                EF.Property<NpgsqlTsVector>(p, "SearchVector")
                    .Matches(EF.Functions.WebSearchToTsQuery("english", searchTerm))
            )
            .OrderByDescending(p => 
                EF.Property<NpgsqlTsVector>(p, "SearchVector")
                    .Rank(EF.Functions.WebSearchToTsQuery("english", searchTerm))
            )
            .Take(20)
            .ToListAsync(cancellationToken: ct);

        return packages
            .Select(p => new PackageMetadata(
                Name: p.Id, 
                Url: GetWingetPackageUrlFromGithub(p.Name),
                LastVersion:  p.LastVersion ?? "<unknown>",
                Summary: $"Package name in Winget: {p.Name}",
                Versions: p.Versions))
            .ToList();
    }

    private static string GetWingetPackageUrlFromGithub(string? name) =>
        $"https://github.com/microsoft/winget-pkgs/tree/master/manifests/{name?.ToLowerInvariant().FirstOrDefault()}";
}