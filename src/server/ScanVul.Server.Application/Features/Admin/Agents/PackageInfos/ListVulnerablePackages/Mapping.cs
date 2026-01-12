using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.Cve.ValueObjects.Descriptions;

namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.ListVulnerablePackages;

public static class Mapping
{
    public static VulnerablePackageResponse MapToResponse(this VulnerablePackage p, CveDescriptionDocument doc)
    {
        var cnaCvss31 = doc.Payload.Containers
            .Cna?.Metrics
            .FirstOrDefault(x => x.CvssV31 is not null)?
            .CvssV31?.BaseScore;
        
        var cnaCvss30 = doc.Payload.Containers
            .Cna?.Metrics
            .FirstOrDefault(x => x.CvssV30 is not null)?
            .CvssV30?.BaseScore;
        
        var cnaCvss20 = doc.Payload.Containers
            .Cna?.Metrics
            .FirstOrDefault(x => x.CvssV20 is not null)?
            .CvssV20?.BaseScore;
        
        var adpCvss31 = doc.Payload.Containers
            .Adp.SelectMany(x => x.Metrics)
            .FirstOrDefault(x => x.CvssV31 is not null)?
            .CvssV31?.BaseScore;

        var adpCvss30 = doc.Payload.Containers
            .Adp.SelectMany(x => x.Metrics)
            .FirstOrDefault(x => x.CvssV30 is not null)?
            .CvssV30?.BaseScore;
        
        var adpCvss20 = doc.Payload.Containers
            .Adp.SelectMany(x => x.Metrics)
            .FirstOrDefault(x => x.CvssV20 is not null)?
            .CvssV20?.BaseScore;

        var cnaDescription = doc.Payload.Containers
            .Cna?.Descriptions
            .FirstOrDefault(x => x.Lang == "en")?
            .Value;
        
        var adpDescription = doc.Payload.Containers
            .Adp.SelectMany(x => x.Descriptions)
            .FirstOrDefault(x => x.Lang == "en")?
            .Value;
        
        return new VulnerablePackageResponse(
            Id: p.Id,
            CveId: p.CveId,
            PackageId: p.PackageInfoId,
            PackageName: p.PackageInfo.Name,
            PackageVersion: p.PackageInfo.Version,
            CvssV31: cnaCvss31 ?? adpCvss31,
            CvssV30: cnaCvss30 ?? adpCvss30,
            CvssV20: cnaCvss20 ?? adpCvss20,
            Description: cnaDescription ?? adpDescription);
    }
}