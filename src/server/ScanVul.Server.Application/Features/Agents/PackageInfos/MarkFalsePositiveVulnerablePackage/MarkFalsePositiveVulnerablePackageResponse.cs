namespace ScanVul.Server.Application.Features.Agents.PackageInfos.MarkFalsePositiveVulnerablePackage;

/// <summary>
/// Mark false positive vulnerable package response
/// </summary>
/// <param name="Id">Vulnerable package ID</param>
/// <param name="CveId">CVE</param>
/// <param name="PackageId">Package ID</param>
/// <param name="PackageName">Package name</param>
/// <param name="PackageVersion">Package version</param>
public record MarkFalsePositiveVulnerablePackageResponse(
    long Id,
    string CveId,
    long PackageId,
    string PackageName,
    string PackageVersion,
    bool IsFalsePositive);