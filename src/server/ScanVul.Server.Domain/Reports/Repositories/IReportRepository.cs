using ScanVul.Server.Domain.Reports.Entities;

namespace ScanVul.Server.Domain.Reports.Repositories;

public interface IReportRepository
{
    Task<List<VulnerabilityScanReport>> GetReportsAsync(CancellationToken ct = default);
    Task<VulnerabilityScanReport?> GetReportByIdAsync(long id, CancellationToken ct = default);
}