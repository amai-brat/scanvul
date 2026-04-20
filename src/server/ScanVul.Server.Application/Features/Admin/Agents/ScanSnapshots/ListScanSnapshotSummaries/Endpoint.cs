using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Repositories;

namespace ScanVul.Server.Application.Features.Admin.Agents.ScanSnapshots.ListScanSnapshotSummaries;

public class ListScanSnapshotSummariesEndpoint(
    IAgentRepository agentRepository)
    : Endpoint<ListScanSnapshotSummariesRequest, Results<Ok<ListScanSnapshotSummariesResponse>, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Get("api/{apiVersion}/admin/agents/{agentId}/snapshots/summary");
        Summary(s =>
        {
            s.Summary = "Get scan snapshot summaries";
            s.Description = "Get scan snapshot summaries of agent";
        });
        Description(x => x
            .WithTags("Admin")
            .Produces<ProblemDetails>(404, "application/problem+json"));
    }
    
    public override async Task<Results<Ok<ListScanSnapshotSummariesResponse>, ProblemDetails>> ExecuteAsync(
        ListScanSnapshotSummariesRequest req,
        CancellationToken ct)
    {
        var agent = await agentRepository.GetWithScanSnapshotsNoTrackingAsync(req.AgentId, ct);
        if (agent is null)
        {
            AddError(x => x.AgentId, "Agent not found");
            return new ProblemDetails(ValidationFailures, (int) HttpStatusCode.NotFound);
        }

        var summaries = agent.Computer.Snapshots
            .Select(x => x.ToResponse())
            .ToList();
        
        return TypedResults.Ok(new ListScanSnapshotSummariesResponse(summaries));
    }
}