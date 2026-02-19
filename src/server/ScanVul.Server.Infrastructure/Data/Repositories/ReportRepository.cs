using Microsoft.EntityFrameworkCore;
using ScanVul.Server.Domain.Reports.Entities;
using ScanVul.Server.Domain.Reports.Repositories;

namespace ScanVul.Server.Infrastructure.Data.Repositories;

public class ReportRepository(AppDbContext dbContext) : IReportRepository
{
    public async Task<List<VulnerabilityScanReport>> GetReportsAsync(CancellationToken ct = default)
    {
        var reports = await dbContext.VulnerabilityScanReports.ToListAsync(ct);
        return reports;
    }

    public async Task<VulnerabilityScanReport?> GetReportByIdAsync(long id, CancellationToken ct = default)
    {
        var report = await dbContext.VulnerabilityScanReports.FirstOrDefaultAsync(r => r.Id == id, ct);
        return report;
    }
}