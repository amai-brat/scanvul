using Microsoft.AspNetCore.Mvc;
using ScanVul.Server.Domain.AgentAggregate.Enums;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.EditVulnerablePackage;

/// <summary>
/// Edit vulnerable package request
/// </summary>
/// <param name="VulnerablePackageId">Vulnerable package ID</param>
/// <param name="Status">Status</param>
public record EditVulnerablePackageRequest(
    [FromRoute(Name = "vulnerablePackageId")] long VulnerablePackageId,
    VulnerablePackageStatus? Status);