using ScanVul.Server.Domain.AgentAggregate.Entities;

namespace ScanVul.Server.Domain.AgentAggregate.Repositories;

public interface IAgentRepository
{
    Task<Agent> AddAsync(Agent agent, CancellationToken ct = default);
    Task<Agent?> GetByTokenWithComputerAsync(Guid token, CancellationToken ct = default);
    Task<Agent?> GetByTokenWithComputerPackagesAsync(Guid token, CancellationToken ct = default);
    Task<Agent?> GetByTokenWithNotSentCommandsAsync(Guid token, CancellationToken ct = default);
    Task<Agent?> GetByTokenWithCommandAsync(Guid token, Guid commandId, CancellationToken ct = default);
    Task<Agent?> GetWithCommandsAsync(long agentId, CancellationToken ct = default);
    
    Task<List<Agent>> GetAllWithComputerNoTrackingAsync(CancellationToken ct = default);
    Task<Agent?> GetWithPackagesNoTrackingAsync(long agentId, CancellationToken ct = default);
    
    /// <summary>
    /// Get agent with vulnerable packages excluding false positives
    /// </summary>
    /// <param name="agentId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<Agent?> GetWithVulnerablePackagesNoTrackingAsync(long agentId, CancellationToken ct = default);
}