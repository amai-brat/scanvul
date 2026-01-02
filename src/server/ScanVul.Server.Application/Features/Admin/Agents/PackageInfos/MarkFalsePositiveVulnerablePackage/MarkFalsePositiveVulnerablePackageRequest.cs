using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.MarkFalsePositiveVulnerablePackage;

/// <summary>
/// Mark false positive vulnerable package request
/// </summary>
/// <param name="VulnerablePackageId">Vulnerable package ID</param>
public record MarkFalsePositiveVulnerablePackageRequest(
    [FromRoute(Name = "vulnerablePackageId")] long VulnerablePackageId);