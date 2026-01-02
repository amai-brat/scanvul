using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.MarkFalsePositiveVulnerablePackage;

public class MarkFalsePositiveVulnerablePackageEndpoint(
    IPackageInfoRepository packageRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<
        MarkFalsePositiveVulnerablePackageRequest, 
        Results<Ok<MarkFalsePositiveVulnerablePackageResponse>, ProblemDetails>
    >
{
    public override void Configure()
    {
        Version(1);
        Patch("api/{apiVersion}/admin/agents/vulnerable-packages/{vulnerablePackageId}/false-positive");
        Summary(s =>
        {
            s.Summary = "Mark false positive vulnerable package";
            s.Description = "Mark false positive vulnerable package";
            s.ExampleRequest = new MarkFalsePositiveVulnerablePackageRequest(-1);
        });
        Description(x => x
            .WithTags("Admin")
            .Produces<ProblemDetails>(404, "application/problem+json"));
    }
    
    public override async Task<Results<Ok<MarkFalsePositiveVulnerablePackageResponse>, ProblemDetails>> ExecuteAsync(
        MarkFalsePositiveVulnerablePackageRequest req,
        CancellationToken ct)
    {
        var vulnerablePackage = await packageRepository.GetVulnerableByIdAsync(req.VulnerablePackageId, ct);
        if (vulnerablePackage is null)
        {
            AddError(x => x.VulnerablePackageId, $"Vulnerable package {req.VulnerablePackageId} not found");
            return new ProblemDetails(ValidationFailures, (int) HttpStatusCode.NotFound);
        }
        
        vulnerablePackage.IsFalsePositive = true;
        
        await unitOfWork.SaveChangesAsync(ct);
        
        return TypedResults.Ok(vulnerablePackage.MapToResponse());
    }
}