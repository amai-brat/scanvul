namespace ScanVul.Server.Application.Features.Agents.PackageInfos.ListPackages;

public record ListPackagesResponse(List<PackageResponse> Packages);

public record PackageResponse(
    long Id,
    string Name,
    string Version);
    