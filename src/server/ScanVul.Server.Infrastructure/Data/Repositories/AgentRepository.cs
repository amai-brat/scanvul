using ScanVul.Server.Domain.Entities;
using ScanVul.Server.Domain.Repositories;

namespace ScanVul.Server.Infrastructure.Data.Repositories;

public class AgentRepository(AppDbContext dbContext) : IAgentRepository
{
    public Task<Agent> AddAsync(Agent agent, CancellationToken ct = default)
    {
        var entry = dbContext.Agents.Add(agent);
        return Task.FromResult(entry.Entity);
    }
}