using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ScanVul.Server.Domain.PackageManagers.Services;
using ScanVul.Server.Domain.PackageManagers.ValueObjects;

namespace ScanVul.Server.Infrastructure.Choco.Services;

public class AltLinuxRpmPackageManager(
    HttpClient httpClient,
    ILogger<AltLinuxRpmPackageManager> logger) : IPackageManager
{
    private const string BaseApiUrl = "https://rdb.altlinux.org/api/site/find_packages";
    
    public async Task<List<PackageMetadata>> SearchAsync(string searchTerm, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return [];
        }

        var requestUrl = $"{BaseApiUrl}?name={Uri.EscapeDataString(searchTerm)}";
        try
        {
            var response = await httpClient.GetFromJsonAsync<AltFindResponse>(requestUrl, ct);
            if (response?.Packages == null)
            {
                return [];
            }

            return response.Packages
                .Take(20)
                .Select(MapToMetadata)
                .ToList();
        }
        catch (HttpRequestException ex)
        {
            logger.LogInformation(ex, "Error when searching from AltLinux RPM packages");
            return [];
        }
    }
    
    private static PackageMetadata MapToMetadata(AltPackageDto pkg)
    {
        var bestVersion = pkg.Versions.FirstOrDefault(v => 
                              string.Equals(v.Branch, "Sisyphus", StringComparison.OrdinalIgnoreCase)) 
                          ?? pkg.Versions.FirstOrDefault();

        string fullVersion;
        string branchName;

        if (bestVersion != null)
        {
            fullVersion = $"{bestVersion.Version}-{bestVersion.Release}";
            branchName = bestVersion.Branch;
        }
        else
        {
            fullVersion = "0.0.0";
            branchName = "Sisyphus";
        }

        // 2. Construct the URL to the Alt Linux packages website.
        // Format: https://packages.altlinux.org/en/{branch}/srpms/{name}
        // We use 'srpms' (Source RPMs) as the main landing page for a package name,
        // as 'rpms' often requires the specific binary sub-package name.
        var packageUrl = $"https://packages.altlinux.org/en/{branchName}/srpms/{pkg.Name}";
        return new PackageMetadata(
            Name: pkg.Name,
            Url: packageUrl,
            LastVersion: fullVersion,
            Summary: pkg.Summary ?? string.Empty,
            Versions: [fullVersion]
        );
    }

    private class AltFindResponse
    {
        [JsonPropertyName("request_args")]
        public object? RequestArgs { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("packages")]
        public List<AltPackageDto>? Packages { get; set; }
    }

    private class AltPackageDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("buildtime")]
        public long Buildtime { get; set; }

        // Note: This URL in the JSON usually points to the upstream homepage (e.g. google.com),
        // not the AltLinux repository page. We ignore it in favor of the constructed Url above.
        [JsonPropertyName("url")]
        public string? Url { get; set; } 

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("versions")]
        public List<AltVersionDto> Versions { get; set; } = new();

        [JsonPropertyName("by_binary")]
        public bool ByBinary { get; set; }
    }

    private class AltVersionDto
    {
        [JsonPropertyName("branch")]
        public string Branch { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("release")]
        public string Release { get; set; } = string.Empty;

        [JsonPropertyName("pkghash")]
        public string? PkgHash { get; set; }

        [JsonPropertyName("deleted")]
        public bool Deleted { get; set; }
    }
}