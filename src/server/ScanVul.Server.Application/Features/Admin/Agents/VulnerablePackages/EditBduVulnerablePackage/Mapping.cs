using ScanVul.Server.Domain.AgentAggregate.Entities;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.EditBduVulnerablePackage;

public static class Mapping
{
    public static EditBduVulnerablePackageResponse MapToResponse(this BduVulnerablePackage p)
    {
        return new EditBduVulnerablePackageResponse(
            Id: p.Id,
            BduId: p.VulnerabilityId,
            PackageId: p.PackageInfoId,
            PackageName: p.PackageInfo.Name,
            PackageVersion: p.PackageInfo.Version,
            Status: p.Status);
    }
}