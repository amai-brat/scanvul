using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.Reports.Models;

namespace ScanVul.Server.Domain.Reports.Services;

public interface IReportModelsGenerator
{
    Task<SeverityStatsModel> GetCveSeverityStatsModelAsync(List<VulnerablePackage> vulnerablePackages, CancellationToken ct = default);
    Task<SeverityStatsModel> GetBduSeverityStatsModelAsync(List<BduVulnerablePackage> vulnerablePackages, CancellationToken ct = default);
    Task<AgentInfoModel> GetAgentInfoModelAsync(Agent agent, CancellationToken ct = default);
}