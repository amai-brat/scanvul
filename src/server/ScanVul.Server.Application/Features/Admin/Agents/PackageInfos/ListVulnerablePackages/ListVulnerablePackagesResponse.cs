namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.ListVulnerablePackages;

/// <summary>
/// List vulnerable packages response
/// </summary>
/// <param name="Packages">Vulnerable packages</param>
public record ListVulnerablePackagesResponse(List<VulnerablePackageResponse> Packages);

/// <summary>
/// Vulnerable package
/// </summary>
/// <param name="Id">Vulnerable package ID</param>
/// <param name="CveId">CVE</param>
/// <param name="PackageId">Package ID</param>
/// <param name="PackageName">Package name</param>
/// <param name="PackageVersion">Package version</param>
public record VulnerablePackageResponse(
    long Id,
    string CveId,
    long PackageId,
    string PackageName,
    string PackageVersion);
    