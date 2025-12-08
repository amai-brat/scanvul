using Microsoft.EntityFrameworkCore;
using ScanVul.Server.Domain.Entities;
using ScanVul.Server.Domain.Repositories;

namespace ScanVul.Server.Infrastructure.Data.Repositories;

public class ComputerRepository(AppDbContext dbContext) : IComputerRepository
{
    public async Task<Computer?> GetComputerWithAllPackagesAsync(long computerId, CancellationToken ct = default)
    {
        var computer = await dbContext.Computers
            .Include(x => x.Packages)
            .Include(x => x.VulnerablePackages)
            .FirstOrDefaultAsync(x => x.Id == computerId, ct);

        return computer;
    }
}