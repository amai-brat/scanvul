using ScanVul.Server.Domain.AgentAggregate.Entities;

namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.MarkFalsePositiveVulnerablePackage;

public static class Mapping
{
    public static MarkFalsePositiveVulnerablePackageResponse MapToResponse(this VulnerablePackage p)
    {
        return new MarkFalsePositiveVulnerablePackageResponse(
            Id: p.Id,
            CveId: p.CveId,
            PackageId: p.PackageInfoId,
            PackageName: p.PackageInfo.Name,
            PackageVersion: p.PackageInfo.Version,
            IsFalsePositive: p.IsFalsePositive);
    }
}