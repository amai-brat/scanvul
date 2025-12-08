using ScanVul.Server.Domain.Entities;

namespace ScanVul.Server.Domain.Repositories;

public interface IComputerRepository
{
    Task<Computer?> GetComputerWithAllPackagesAsync(long computerId, CancellationToken ct = default);
}