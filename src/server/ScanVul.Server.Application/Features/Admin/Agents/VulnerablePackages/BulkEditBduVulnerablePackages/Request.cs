using ScanVul.Server.Domain.AgentAggregate.Enums;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.BulkEditBduVulnerablePackages;

/// <summary>
/// Edit BDU vulnerable packages request
/// </summary>
/// <param name="VulnerablePackageIds">BDU vulnerable package Ids</param>
/// <param name="Status">Status</param>
public record BulkEditBduVulnerablePackagesRequest(
    List<long> VulnerablePackageIds,
    VulnerablePackageStatus? Status);