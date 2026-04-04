using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Application.Services;
using ScanVul.Server.Domain.AgentAggregate.Repositories;

namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.ScanPackages;

public class ScanPackagesEndpoint(
    IAgentRepository agentRepository,
    ScannerJobDispatcher scannerJobDispatcher)
    : Endpoint<ScanPackagesRequest, Results<Ok, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Post("api/{apiVersion}/admin/agents/{agentId}/packages/scan");
        Summary(s =>
        {
            s.Summary = "Scan packages of agent";
            s.Description = "Scan packages of agent (CVE and БДУ)";
        });
        Description(x => x
            .WithTags("Admin")
            .Accepts<ScanPackagesRequest>()
            .Produces<ProblemDetails>(404, "application/problem+json"));
    }
    
    public override async Task<Results<Ok, ProblemDetails>> ExecuteAsync(
        ScanPackagesRequest req,
        CancellationToken ct)
    {
        var agent = await agentRepository.GetWithComputerAsync(req.AgentId, ct);
        if (agent is null)
        {
            AddError(x => x.AgentId, "Agent not found");
            return new ProblemDetails(ValidationFailures, (int) HttpStatusCode.NotFound);
        }

        scannerJobDispatcher.DispatchScan(agent.ComputerId);
        
        return TypedResults.Ok();
    }
}