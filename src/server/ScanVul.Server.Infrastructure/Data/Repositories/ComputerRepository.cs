using Microsoft.EntityFrameworkCore;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.AgentAggregate.Repositories;

namespace ScanVul.Server.Infrastructure.Data.Repositories;

public class ComputerRepository(AppDbContext dbContext) : IComputerRepository
{
    public async Task<Computer?> GetComputerWithAllPackagesAsync(long computerId, CancellationToken ct = default)
    {
        var computer = await dbContext.Computers
            .Include(x => x.Packages)
            .Include(x => x.VulnerablePackages)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == computerId, ct);

        return computer;
    }

    public async Task<Computer?> GetComputerWithBduPackagesAsync(long computerId, CancellationToken ct = default)
    {
        var computer = await dbContext.Computers
            .Include(x => x.Packages)
            .Include(x => x.BduVulnerablePackages)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == computerId, ct);

        return computer;
    }
}