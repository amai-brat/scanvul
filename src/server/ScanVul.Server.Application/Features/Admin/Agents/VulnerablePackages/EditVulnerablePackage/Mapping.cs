using ScanVul.Server.Domain.AgentAggregate.Entities;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.EditVulnerablePackage;

public static class Mapping
{
    public static EditVulnerablePackageResponse MapToResponse(this VulnerablePackage p)
    {
        return new EditVulnerablePackageResponse(
            Id: p.Id,
            CveId: p.VulnerabilityId,
            PackageId: p.PackageInfoId,
            PackageName: p.PackageInfo.Name,
            PackageVersion: p.PackageInfo.Version,
            IsFalsePositive: p.IsFalsePositive,
            IsPatchless: p.IsPatchless);
    }
}