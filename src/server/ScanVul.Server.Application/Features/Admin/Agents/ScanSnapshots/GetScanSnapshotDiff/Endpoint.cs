using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Repositories;

namespace ScanVul.Server.Application.Features.Admin.Agents.ScanSnapshots.GetScanSnapshotDiff;

public class GetScanSnapshotPayloadEndpoint(
    ISnapshotRepository snapshotRepository)
    : Endpoint<GetScanSnapshotDiffRequest, Results<Ok<GetScanSnapshotDiffResponse>, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Get("api/{apiVersion}/admin/agents/snapshots/{snapshotId}/diff");
        Summary(s =>
        {
            s.Summary = "Get scan snapshot last diff";
            s.Description = "Get scan snapshot last diff";
        });
        Description(x => x
            .WithTags("Admin")
            .Produces<ProblemDetails>(404, "application/problem+json"));
    }
    
    public override async Task<Results<Ok<GetScanSnapshotDiffResponse>, ProblemDetails>> ExecuteAsync(
        GetScanSnapshotDiffRequest req,
        CancellationToken ct)
    {
        var snapshot = await snapshotRepository.GetWithDiffAsync(req.SnapshotId, ct);
        if (snapshot is null)
        {
            AddError(x => x.SnapshotId, "Snapshot not found");
            return new ProblemDetails(ValidationFailures, (int) HttpStatusCode.NotFound);
        }

        var result = snapshot.ToResponse();
        return TypedResults.Ok(result);
    }
}