using JetBrains.Annotations;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.MarkFalsePositiveBduVulnerablePackage;

/// <summary>
/// Mark false positive BDU vulnerable package response
/// </summary>
/// <param name="Id">Vulnerable package ID</param>
/// <param name="BduId">Номер БДУ</param>
/// <param name="PackageId">Package ID</param>
/// <param name="PackageName">Package name</param>
/// <param name="PackageVersion">Package version</param>
/// <param name="IsFalsePositive">Flag whether package is false positive vulnerable</param>
[PublicAPI]
public record MarkFalsePositiveBduVulnerablePackageResponse(
    long Id,
    string BduId,
    long PackageId,
    string PackageName,
    string PackageVersion,
    bool IsFalsePositive);