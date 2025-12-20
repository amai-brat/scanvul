using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Repositories;

namespace ScanVul.Server.Application.Features.Agents.PackageInfos.ListVulnerablePackages;

public class ListVulnerablePackagesEndpoint(
    IAgentRepository agentRepository)
    : Endpoint<ListVulnerablePackagesRequest, Results<Ok<ListVulnerablePackagesResponse>, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Get("api/{apiVersion}/agents/{agentId}/vulnerable-packages");
        Summary(s =>
        {
            s.Summary = "Get all vulnerable packages on computer of agent";
            s.Description = "Get all vulnerable packages on computer of agent";
        });
        Description(x => x
            .WithTags("Agents")
            .Produces<ProblemDetails>(404, "application/problem+json"));
    }
    
    public override async Task<Results<Ok<ListVulnerablePackagesResponse>, ProblemDetails>> ExecuteAsync(
        ListVulnerablePackagesRequest req,
        CancellationToken ct)
    {
        var agent = await agentRepository.GetWithVulnerablePackagesNoTrackingAsync(req.AgentId, ct);
        if (agent is null)
        {
            AddError(x => x.AgentId, "Agent not found");
            return new ProblemDetails(ValidationFailures, (int) HttpStatusCode.NotFound);
        }

        var packages = agent.Computer.VulnerablePackages
            .Select(p => new VulnerablePackageResponse(
                Id: p.Id, 
                CveId: p.CveId, 
                PackageId: p.PackageInfoId, 
                PackageName: p.PackageInfo.Name, 
                PackageVersion: p.PackageInfo.Version))
            .ToList();
        
        return TypedResults.Ok(new ListVulnerablePackagesResponse(packages));
    }
}