using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Repositories;

namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.ListPackages;

public class ListPackagesEndpoint(
    IAgentRepository agentRepository)
    : Endpoint<ListPackagesRequest, Results<Ok<ListPackagesResponse>, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Get("api/{apiVersion}/admin/agents/{agentId}/packages");
        Summary(s =>
        {
            s.Summary = "Get all packages on computer of agent";
            s.Description = "Get all packages on computer of agent";
        });
        Description(x => x
            .WithTags("Admin")
            .Produces<ProblemDetails>(404, "application/problem+json"));
    }
    
    public override async Task<Results<Ok<ListPackagesResponse>, ProblemDetails>> ExecuteAsync(
        ListPackagesRequest req,
        CancellationToken ct)
    {
        var agent = await agentRepository.GetWithPackagesNoTrackingAsync(req.AgentId, ct);
        if (agent is null)
        {
            AddError(x => x.AgentId, "Agent not found");
            return new ProblemDetails(ValidationFailures, (int) HttpStatusCode.NotFound);
        }

        var packages = agent.Computer.Packages
            .Select(p => new PackageResponse(p.Id, p.Name, p.Version))
            .OrderBy(x => x.Name)
            .ToList();
        
        return TypedResults.Ok(new ListPackagesResponse(packages));
    }
}