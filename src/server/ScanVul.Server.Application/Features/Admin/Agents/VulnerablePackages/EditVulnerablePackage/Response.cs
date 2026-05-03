using JetBrains.Annotations;
using ScanVul.Server.Domain.AgentAggregate.Enums;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.EditVulnerablePackage;

/// <summary>
/// Edit vulnerable package response
/// </summary>
/// <param name="Id">Vulnerable package ID</param>
/// <param name="CveId">CVE</param>
/// <param name="PackageId">Package ID</param>
/// <param name="PackageName">Package name</param>
/// <param name="PackageVersion">Package version</param>
/// <param name="Status">Status</param>
[PublicAPI]
public record EditVulnerablePackageResponse(
    long Id,
    string CveId,
    long PackageId,
    string PackageName,
    string PackageVersion,
    VulnerablePackageStatus Status);