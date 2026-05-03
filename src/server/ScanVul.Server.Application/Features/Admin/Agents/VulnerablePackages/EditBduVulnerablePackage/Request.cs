using Microsoft.AspNetCore.Mvc;
using ScanVul.Server.Domain.AgentAggregate.Enums;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.EditBduVulnerablePackage;

/// <summary>
/// Edit BDU vulnerable package request
/// </summary>
/// <param name="VulnerablePackageId">BDU vulnerable package ID</param>
/// <param name="Status">Status</param>
public record EditBduVulnerablePackageRequest(
    [FromRoute(Name = "vulnerablePackageId")] long VulnerablePackageId,
    VulnerablePackageStatus? Status);