namespace ScanVul.Server.Application.Features.Agents.PackageInfos.ListVulnerablePackages;

public record ListVulnerablePackagesResponse(List<VulnerablePackageResponse> Packages);

public record VulnerablePackageResponse(
    long Id,
    string CveId,
    long PackageId,
    string PackageName,
    string PackageVersion);
    