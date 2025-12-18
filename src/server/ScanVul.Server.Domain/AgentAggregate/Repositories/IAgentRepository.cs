using ScanVul.Server.Domain.AgentAggregate.Entities;

namespace ScanVul.Server.Domain.AgentAggregate.Repositories;

public interface IAgentRepository
{
    Task<Agent> AddAsync(Agent agent, CancellationToken ct = default);
    Task<Agent?> GetByTokenWithComputerAsync(Guid token, CancellationToken ct = default);
    Task<Agent?> GetByTokenWithComputerPackagesAsync(Guid token, CancellationToken ct = default);
}