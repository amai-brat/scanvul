using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ScanVul.Server.Domain.PackageManagers.Services;
using ScanVul.Server.Domain.PackageManagers.ValueObjects;

namespace ScanVul.Server.Infrastructure.Choco.Services;

public class PacmanPackageManager(
    HttpClient httpClient,
    ILogger<PacmanPackageManager> logger) : IPackageManager
{ 
    private const string BaseSearchUrl = "https://archlinux.org/packages/search/json/";
    
    public async Task<List<PackageMetadata>> SearchAsync(string searchTerm, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return [];
        }

        var requestUrl = $"{BaseSearchUrl}?q={Uri.EscapeDataString(searchTerm)}";

        try
        {
            var response = await httpClient.GetFromJsonAsync<ArchApiResponse>(requestUrl, ct);
            if (response?.Results is null)
            {
                return [];
            }

            return response.Results
                .Take(20)
                .Select(MapToMetadata)
                .ToList();
        }
        catch (HttpRequestException ex)
        {
            logger.LogInformation(ex, "Error when searching from ArchLinux packages");
            return [];
        }
    }

    private static PackageMetadata MapToMetadata(ArchPackageDto pkg)
    {
        var fullVersion = $"{pkg.PkgVer}-{pkg.PkgRel}";
        var packageUrl = $"https://archlinux.org/packages/{pkg.Repo}/{pkg.Arch}/{pkg.PkgName}/";

        return new PackageMetadata(
            Name: pkg.PkgName,
            Url: packageUrl, 
            LastVersion: fullVersion,
            Summary: pkg.PkgDesc
        );
    }

    private class ArchApiResponse
    {
        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("valid")]
        public bool Valid { get; set; }

        [JsonPropertyName("results")]
        public List<ArchPackageDto>? Results { get; set; }
    }

    private class ArchPackageDto
    {
        [JsonPropertyName("pkgname")]
        public string PkgName { get; set; } = string.Empty;

        [JsonPropertyName("pkgver")]
        public string PkgVer { get; set; } = string.Empty;

        [JsonPropertyName("pkgrel")]
        public string PkgRel { get; set; } = string.Empty;

        [JsonPropertyName("pkgdesc")]
        public string PkgDesc { get; set; } = string.Empty;

        [JsonPropertyName("repo")]
        public string Repo { get; set; } = string.Empty;

        [JsonPropertyName("arch")]
        public string Arch { get; set; } = string.Empty;
    }
}