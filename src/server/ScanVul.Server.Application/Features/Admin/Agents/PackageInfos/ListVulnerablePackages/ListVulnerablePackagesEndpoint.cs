using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Cve.Repositories;

namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.ListVulnerablePackages;

public class ListVulnerablePackagesEndpoint(
    IAgentRepository agentRepository,
    ICveRepository cveRepository)
    : Endpoint<ListVulnerablePackagesRequest, Results<Ok<ListVulnerablePackagesResponse>, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Get("api/{apiVersion}/admin/agents/{agentId}/vulnerable-packages");
        Summary(s =>
        {
            s.Summary = "Get all vulnerable packages on computer of agent";
            s.Description = "Get all vulnerable packages on computer of agent";
        });
        Description(x => x
            .WithTags("Admin")
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

        var cveDescriptions = await cveRepository.GetCveDescriptionDocumentsAsync(
            agent.Computer
                .VulnerablePackages
                .Select(x => x.CveId), 
            ct);

        var descriptionDic = cveDescriptions
            .ToDictionary(x => x.Payload.CveMetadata.CveId);
        
        var packages = agent.Computer.VulnerablePackages
            .Select(p => p.MapToResponse(descriptionDic[p.CveId]))
            .OrderBy(x => x.PackageName)
            .ToList();
        
        return TypedResults.Ok(new ListVulnerablePackagesResponse(packages));
    }
}