using ScanVul.Server.Domain.AgentAggregate.Entities;

namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.MarkFalsePositiveBduVulnerablePackage;

public static class Mapping
{
    public static MarkFalsePositiveBduVulnerablePackageResponse MapToResponse(this BduVulnerablePackage p)
    {
        return new MarkFalsePositiveBduVulnerablePackageResponse(
            Id: p.Id,
            BduId: p.BduId,
            PackageId: p.PackageInfoId,
            PackageName: p.PackageInfo.Name,
            PackageVersion: p.PackageInfo.Version,
            IsFalsePositive: p.IsFalsePositive);
    }
}