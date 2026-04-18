using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.EditBduVulnerablePackage;

/// <summary>
/// Edit BDU vulnerable package request
/// </summary>
/// <param name="VulnerablePackageId">BDU vulnerable package ID</param>
/// <param name="IsFalsePositive">Package is marked as vulnerable falsely</param>
/// <param name="IsPatchless">Vulnerable package doesn't have patches to fix vulnerablity currently</param>
public record EditBduVulnerablePackageRequest(
    [FromRoute(Name = "vulnerablePackageId")] long VulnerablePackageId,
    bool? IsFalsePositive,
    bool? IsPatchless);