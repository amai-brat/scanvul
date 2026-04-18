using System.Linq.Expressions;
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
    Task<Agent?> GetWithComputerAsync(long agentId, CancellationToken ct = default);
    
    Task<List<Agent>> GetActiveAgentsWithComputerNoTrackingAsync(CancellationToken ct = default);
    Task<Agent?> GetWithPackagesNoTrackingAsync(long agentId, CancellationToken ct = default);
    Task<Agent?> GetWithCommandsNoTrackingAsync(long agentId, CancellationToken ct = default);
    
    /// <summary>
    /// Get agent with vulnerable packages with filters
    /// </summary>
    Task<Agent?> GetWithVulnerablePackagesNoTrackingAsync(
        long agentId, 
        Expression<Func<VulnerablePackage, bool>> vulnerablePackageFilter, 
        CancellationToken ct = default);
    
    /// <summary>
    /// Get agent with BDU vulnerable packages with filters
    /// </summary>
    Task<Agent?> GetWithBduVulnerablePackagesNoTrackingAsync(
        long agentId,
        Expression<Func<BduVulnerablePackage, bool>> vulnerablePackageFilter, 
        CancellationToken ct = default);
}