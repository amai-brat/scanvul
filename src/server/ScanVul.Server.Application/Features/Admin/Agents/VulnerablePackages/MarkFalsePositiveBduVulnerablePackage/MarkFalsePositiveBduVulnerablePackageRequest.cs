using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.MarkFalsePositiveBduVulnerablePackage;

/// <summary>
/// Mark false positive BDU vulnerable package request
/// </summary>
/// <param name="VulnerablePackageId">BDU vulnerable package ID</param>
public record MarkFalsePositiveBduVulnerablePackageRequest(
    [FromRoute(Name = "vulnerablePackageId")] long VulnerablePackageId);