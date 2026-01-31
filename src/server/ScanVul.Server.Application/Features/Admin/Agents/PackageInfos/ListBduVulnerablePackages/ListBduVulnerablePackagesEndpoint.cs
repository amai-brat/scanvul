using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Cve.Repositories;

namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.ListBduVulnerablePackages;

public class ListBduVulnerablePackagesEndpoint(
    IAgentRepository agentRepository,
    IBduRepository bduRepository)
    : Endpoint<ListBduVulnerablePackagesRequest, Results<Ok<ListBduVulnerablePackagesResponse>, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Get("api/{apiVersion}/admin/agents/{agentId}/vulnerable-packages/bdu");
        Summary(s =>
        {
            s.Summary = "Get all БДУ vulnerable packages on computer of agent";
            s.Description = "Get all БДУ vulnerable packages on computer of agent";
        });
        Description(x => x
            .WithTags("Admin")
            .Produces<ProblemDetails>(404, "application/problem+json"));
    }
    
    public override async Task<Results<Ok<ListBduVulnerablePackagesResponse>, ProblemDetails>> ExecuteAsync(
        ListBduVulnerablePackagesRequest req,
        CancellationToken ct)
    {
        var agent = await agentRepository.GetWithBduVulnerablePackagesNoTrackingAsync(req.AgentId, ct);
        if (agent is null)
        {
            AddError(x => x.AgentId, "Agent not found");
            return new ProblemDetails(ValidationFailures, (int) HttpStatusCode.NotFound);
        }
        
        var bduDescriptions = await bduRepository.GetBduDescriptionDocumentsAsync(
            agent.Computer
                .BduVulnerablePackages
                .Select(x => x.BduId), 
            ct);

        var descriptionDic = bduDescriptions
            .ToDictionary(x => x.Identifier.First());
        
        var packages = agent.Computer.BduVulnerablePackages
            .Select(p => p.MapToResponse(descriptionDic[p.BduId]))
            .OrderBy(x => x.PackageName)
            .ThenBy(x => x.BduId)
            .ToList();
        
        return TypedResults.Ok(new ListBduVulnerablePackagesResponse(packages));
    }
}