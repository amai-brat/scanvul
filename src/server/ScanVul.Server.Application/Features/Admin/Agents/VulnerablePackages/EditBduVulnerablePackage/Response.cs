using JetBrains.Annotations;
using ScanVul.Server.Domain.AgentAggregate.Enums;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.EditBduVulnerablePackage;

/// <summary>
/// Edit BDU vulnerable package response
/// </summary>
/// <param name="Id">BDU vulnerable package ID</param>
/// <param name="BduId">БДУ</param>
/// <param name="PackageId">Package ID</param>
/// <param name="PackageName">Package name</param>
/// <param name="PackageVersion">Package version</param>
/// <param name="Status">Status</param>
[PublicAPI]
public record EditBduVulnerablePackageResponse(
    long Id,
    string BduId,
    long PackageId,
    string PackageName,
    string PackageVersion,
    VulnerablePackageStatus Status);