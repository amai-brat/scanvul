using ScanVul.Server.Domain.AgentAggregate.Entities;

namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.ListVulnerablePackages;

public static class Mapping
{
    public static VulnerablePackageResponse MapToResponse(this VulnerablePackage p)
    {
        return new VulnerablePackageResponse(
            Id: p.Id,
            CveId: p.CveId,
            PackageId: p.PackageInfoId,
            PackageName: p.PackageInfo.Name,
            PackageVersion: p.PackageInfo.Version);
    }
}