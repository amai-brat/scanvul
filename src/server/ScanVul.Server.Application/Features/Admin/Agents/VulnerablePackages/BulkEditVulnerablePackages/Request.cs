using ScanVul.Server.Domain.AgentAggregate.Enums;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.BulkEditVulnerablePackages;

/// <summary>
/// Edit vulnerable packages request
/// </summary>
/// <param name="VulnerablePackageIds">Vulnerable package Ids</param>
/// <param name="Status">Status</param>
public record BulkEditVulnerablePackagesRequest(
    List<long> VulnerablePackageIds,
    VulnerablePackageStatus? Status);