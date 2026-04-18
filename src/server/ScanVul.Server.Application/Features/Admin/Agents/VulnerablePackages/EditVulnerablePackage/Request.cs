using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.EditVulnerablePackage;

/// <summary>
/// Edit vulnerable package request
/// </summary>
/// <param name="VulnerablePackageId">Vulnerable package ID</param>
/// <param name="IsFalsePositive">Package is marked as vulnerable falsely</param>
/// <param name="IsPatchless">Vulnerable package doesn't have patches to fix vulnerablity currently</param>
public record EditVulnerablePackageRequest(
    [FromRoute(Name = "vulnerablePackageId")] long VulnerablePackageId,
    bool? IsFalsePositive,
    bool? IsPatchless);