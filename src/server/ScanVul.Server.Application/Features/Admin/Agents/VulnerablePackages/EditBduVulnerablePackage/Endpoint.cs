using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Enums;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.EditBduVulnerablePackage;

public class EditBduVulnerablePackageEndpoint(
    IPackageInfoRepository packageRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<
        EditBduVulnerablePackageRequest, 
        Results<Ok<EditBduVulnerablePackageResponse>, ProblemDetails>
    >
{
    public override void Configure()
    {
        Version(1);
        Patch("api/{apiVersion}/admin/agents/bdu-vulnerable-packages/{vulnerablePackageId}");
        Summary(s =>
        {
            s.Summary = "Edit BDU vulnerable package";
            s.Description = "Edit BDU vulnerable package";
            s.ExampleRequest = new EditBduVulnerablePackageRequest(
                VulnerablePackageId: -1, 
                Status: VulnerablePackageStatus.FalsePositive);
        });
        Description(x => x
            .WithTags("Admin")
            .Accepts<EditBduVulnerablePackageRequest>("application/json")
            .Produces<ProblemDetails>(404, "application/problem+json"));
    }
    
    public override async Task<Results<Ok<EditBduVulnerablePackageResponse>, ProblemDetails>> ExecuteAsync(
        EditBduVulnerablePackageRequest req,
        CancellationToken ct)
    {
        var vulnerablePackage = await packageRepository.GetBduVulnerableByIdAsync(req.VulnerablePackageId, ct);
        if (vulnerablePackage is null)
        {
            AddError(x => x.VulnerablePackageId, $"Vulnerable package {req.VulnerablePackageId} not found");
            return new ProblemDetails(ValidationFailures, (int) HttpStatusCode.NotFound);
        }
        
        if (req.Status is not null)
            vulnerablePackage.Status = req.Status.Value;
        
        await unitOfWork.SaveChangesAsync(ct);
        
        return TypedResults.Ok(vulnerablePackage.MapToResponse());
    }
}