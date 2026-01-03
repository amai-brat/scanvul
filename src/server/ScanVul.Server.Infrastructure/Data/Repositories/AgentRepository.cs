using Microsoft.EntityFrameworkCore;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.AgentAggregate.Repositories;

namespace ScanVul.Server.Infrastructure.Data.Repositories;

public class AgentRepository(AppDbContext dbContext) : IAgentRepository
{
    public Task<Agent> AddAsync(Agent agent, CancellationToken ct = default)
    {
        var entry = dbContext.Agents.Add(agent);
        return Task.FromResult(entry.Entity);
    }

    public async Task<Agent?> GetByTokenWithComputerAsync(Guid token, CancellationToken ct = default)
    {
        var agent = await dbContext.Agents
            .Include(x => x.Computer)
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken: ct);
        return agent;
    }

    public async Task<Agent?> GetByTokenWithComputerPackagesAsync(Guid token, CancellationToken ct = default)
    {
        var agent = await dbContext.Agents
            .Include(x => x.Computer)
                .ThenInclude(x => x.Packages)
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken: ct);
        return agent;
    }

    public async Task<Agent?> GetByTokenWithNotSentCommandsAsync(Guid token, CancellationToken ct = default)
    {
        var agent = await dbContext.Agents
            .Include(x =>  x.Commands
                .Where(c => c.SentAt == null))
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken: ct);
        return agent;
    }

    public async Task<Agent?> GetByTokenWithCommandAsync(Guid token, Guid commandId, CancellationToken ct = default)
    {
        var agent = await dbContext.Agents
            .Include(x =>  x.Commands
                .Where(c => c.Id == commandId))
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken: ct);
        return agent;
    }

    public async Task<Agent?> GetWithCommandsAsync(long agentId, CancellationToken ct = default)
    {
        var agent = await dbContext.Agents
            .Include(x => x.Commands)
            .FirstOrDefaultAsync(x => x.Id == agentId, cancellationToken: ct);
        return agent;
    }

    public async Task<List<Agent>> GetActiveAgentsWithComputerNoTrackingAsync(CancellationToken ct = default)
    {
        return await dbContext.Agents
            .Where(x => x.IsActive)
            .Include(x => x.Computer)
            .AsNoTracking()
            .OrderByDescending(x => x.LastPingAt)
            .ToListAsync(ct);
    }

    public async Task<Agent?> GetWithPackagesNoTrackingAsync(long agentId, CancellationToken ct = default)
    {
        var agent = await dbContext.Agents
            .Include(x => x.Computer)
                .ThenInclude(x => x.Packages)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == agentId, cancellationToken: ct);
        return agent;
    }

    public async Task<Agent?> GetWithVulnerablePackagesNoTrackingAsync(long agentId, CancellationToken ct = default)
    {
        var agent = await dbContext.Agents
            .Include(x => x.Computer)
                .ThenInclude(x => x.VulnerablePackages.Where(vp => !vp.IsFalsePositive))
                    .ThenInclude(x => x.PackageInfo)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == agentId, cancellationToken: ct);
        return agent;
    }
}