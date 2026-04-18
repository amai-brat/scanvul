using JetBrains.Annotations;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.EditVulnerablePackage;

/// <summary>
/// Edit vulnerable package response
/// </summary>
/// <param name="Id">Vulnerable package ID</param>
/// <param name="CveId">CVE</param>
/// <param name="PackageId">Package ID</param>
/// <param name="PackageName">Package name</param>
/// <param name="PackageVersion">Package version</param>
/// <param name="IsFalsePositive">Flag whether package is false positive vulnerable</param>
/// <param name="IsPatchless">Flag whether package doesn't have patches to fix vulnerablity currently</param>
[PublicAPI]
public record EditVulnerablePackageResponse(
    long Id,
    string CveId,
    long PackageId,
    string PackageName,
    string PackageVersion,
    bool IsFalsePositive,
    bool IsPatchless);