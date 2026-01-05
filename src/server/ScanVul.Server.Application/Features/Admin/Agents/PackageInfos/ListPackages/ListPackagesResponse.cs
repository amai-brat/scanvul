namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.ListPackages;

/// <summary>
/// List packages response
/// </summary>
/// <param name="Packages">Packages</param>
public record ListPackagesResponse(List<PackageResponse> Packages);

/// <summary>
/// Package
/// </summary>
/// <param name="Id">Package ID</param>
/// <param name="Name">Package name</param>
/// <param name="Version">Package version</param>
public record PackageResponse(
    long Id,
    string Name,
    string Version);
    