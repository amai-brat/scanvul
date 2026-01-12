using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Repositories;

namespace ScanVul.Server.Application.Features.Admin.Agents.ListAgents;

public class ListAgentsEndpoint(
    IAgentRepository agentRepository)
    : EndpointWithoutRequest<Ok<ListAgentsResponse>>
{
    public override void Configure()
    {
        Version(1);
        Get("api/{apiVersion}/admin/agents");
        Summary(s =>
        {
            s.Summary = "Get agents";
            s.Description = "Get all active agents";
        });
        Description(x => x.WithTags("Admin"));
    }
    
    public override async Task<Ok<ListAgentsResponse>> ExecuteAsync(
        CancellationToken ct)
    {
        var agents = await agentRepository.GetActiveAgentsWithComputerNoTrackingAsync(ct);

        var dtos = agents
            .Select(agent =>
                new AgentResponse(
                    Id: agent.Id,
                    IsActive: agent.IsActive,
                    LastPingAt: agent.LastPingAt,
                    LastPackagesScrapingAt: agent.LastPackagesScrapingAt,
                    IpAddress: agent.Computer.IpAddress.ToString(),
                    OperatingSystem: agent.Computer.OperatingSystem.ToString(),
                    ComputerName: agent.Computer.Name,
                    MemoryInMb: agent.Computer.MemoryInMb,
                    CpuName: agent.Computer.CpuName
                ))
            .ToList();
        
        return TypedResults.Ok(new ListAgentsResponse(dtos));
    }
}