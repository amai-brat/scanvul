using System.Globalization;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.Cve.Repositories;
using ScanVul.Server.Domain.Cve.ValueObjects.Descriptions;
using ScanVul.Server.Domain.Reports.Models;

namespace ScanVul.Server.Domain.Reports.Services;

public class ReportModelsGenerator(
    ICveRepository cveRepository,
    IBduRepository bduRepository) : IReportModelsGenerator
{
    private static double? GetCveCvssScore(CveDescriptionDocument doc)
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

        return cnaCvss31 ?? adpCvss31 ?? 
               cnaCvss30 ?? adpCvss30 ?? 
               cnaCvss20 ?? adpCvss20;
    }

    private static double? GetBduCvssScore(BduDescriptionDocument doc)
    {
        double? cvssScore = double.TryParse(doc.Cvss?.Vector.Score, NumberStyles.Any, CultureInfo.InvariantCulture, out var score) 
            ? score 
            : null;
        
        double? cvss3Score = double.TryParse(doc.Cvss3?.Vector.Score, NumberStyles.Any, CultureInfo.InvariantCulture, out var score3) 
            ? score3 
            : null;
        
        double? cvss4Score = double.TryParse(doc.Cvss4?.Vector.Score, NumberStyles.Any, CultureInfo.InvariantCulture, out var score4) 
            ? score4 
            : null;

        return cvss4Score ?? cvss3Score ?? cvssScore;
    }

    public async Task<SeverityStatsModel> GetCveSeverityStatsModelAsync(
        List<VulnerablePackage> vulnerablePackages, 
        CancellationToken ct = default)
    {
        var cveDescriptions = await cveRepository.GetCveDescriptionDocumentsAsync(
           vulnerablePackages.Select(x => x.VulnerabilityId), ct);

        var descriptionDic = cveDescriptions
            .ToDictionary(x => x.Payload.CveMetadata.CveId);
        
        var scores = vulnerablePackages
            .Select(p => GetCveCvssScore(descriptionDic[p.VulnerabilityId]))
            .ToList();
        
        int crit = 0, high = 0, medium = 0, low = 0;
        
        foreach (var score in scores)
        {
            if (score is null) continue;
            
            switch (score)
            {
                case >= 9 and <= 10 : crit += 1; break;
                case >= 7 and <   9 : high += 1; break;
                case >= 4 and <   7 : medium += 1; break;
                case >= 0 and <   4 : low += 1; break;
            }
        }
        
        return new SeverityStatsModel(crit,  high, medium, low);
    }

    public async Task<SeverityStatsModel> GetBduSeverityStatsModelAsync(
        List<BduVulnerablePackage> vulnerablePackages, 
        CancellationToken ct = default)
    {
        var bduDescriptions = await bduRepository.GetBduDescriptionDocumentsAsync(
            vulnerablePackages.Select(x => x.VulnerabilityId), ct);

        var descriptionDic = bduDescriptions
            .ToDictionary(x => x.Identifier.First());
        
        var scores = vulnerablePackages
            .Select(p => GetBduCvssScore(descriptionDic[p.VulnerabilityId]))
            .ToList();
        
        int crit = 0, high = 0, medium = 0, low = 0;
        
        foreach (var score in scores)
        {
            if (score is null) continue;
            
            switch (score)
            {
                case >= 9 and <= 10 : crit += 1; break;
                case >= 7 and <   9 : high += 1; break;
                case >= 4 and <   7 : medium += 1; break;
                case >= 0 and <   4 : low += 1; break;
            }
        }
        
        return new SeverityStatsModel(crit,  high, medium, low);
    }

    public async Task<AgentInfoModel> GetAgentInfoModelAsync(
        Agent agent, 
        CancellationToken ct = default)
    {
        var cveStats = await GetCveSeverityStatsModelAsync(agent.Computer.VulnerablePackages, ct);
        var bduStats = await GetBduSeverityStatsModelAsync(agent.Computer.BduVulnerablePackages, ct);

        return new AgentInfoModel(
            Id: agent.Id,
            Name: agent.Computer.Name ?? "Unknown",
            IpAddress: agent.Computer.IpAddress.ToString(),
            OperatingSystem: agent.Computer.OperatingSystem.ToString(),
            PackagesCount: agent.Computer.Packages.Count,
            CveSeverityStats: cveStats,
            BduSeverityStats: bduStats);
    }
}