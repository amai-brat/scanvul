using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Repositories;

namespace ScanVul.Server.Application.Features.Admin.Agents.ScanSnapshots.GetScanSnapshotPayload;

public class GetScanSnapshotPayloadEndpoint(
    ISnapshotRepository snapshotRepository)
    : Endpoint<GetScanSnapshotPayloadRequest, Results<Ok<GetScanSnapshotPayloadResponse>, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Get("api/{apiVersion}/admin/agents/snapshots/{snapshotId}");
        Summary(s =>
        {
            s.Summary = "Get scan snapshot summaries";
            s.Description = "Get scan snapshot summaries of agent";
        });
        Description(x => x
            .WithTags("Admin")
            .Produces<ProblemDetails>(404, "application/problem+json"));
    }
    
    public override async Task<Results<Ok<GetScanSnapshotPayloadResponse>, ProblemDetails>> ExecuteAsync(
        GetScanSnapshotPayloadRequest req,
        CancellationToken ct)
    {
        var snapshot = await snapshotRepository.GetScanSnapshotByIdAsync(req.SnapshotId, ct);
        if (snapshot is null)
        {
            AddError(x => x.SnapshotId, "Snapshot not found");
            return new ProblemDetails(ValidationFailures, (int) HttpStatusCode.NotFound);
        }

        var result = snapshot.ToResponse(req.IncludePayload);
        return TypedResults.Ok(result);
    }
}