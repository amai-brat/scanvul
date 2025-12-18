using ScanVul.Server.Domain.AgentAggregate.Entities;

namespace ScanVul.Server.Domain.AgentAggregate.Repositories;

public interface IComputerRepository
{
    Task<Computer?> GetComputerWithAllPackagesAsync(long computerId, CancellationToken ct = default);
}