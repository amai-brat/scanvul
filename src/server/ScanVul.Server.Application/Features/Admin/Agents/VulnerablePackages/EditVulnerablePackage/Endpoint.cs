using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.EditVulnerablePackage;

public class EditVulnerablePackageEndpoint(
    IPackageInfoRepository packageRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<
        EditVulnerablePackageRequest, 
        Results<Ok<EditVulnerablePackageResponse>, ProblemDetails>
    >
{
    public override void Configure()
    {
        Version(1);
        Patch("api/{apiVersion}/admin/agents/vulnerable-packages/{vulnerablePackageId}");
        Summary(s =>
        {
            s.Summary = "Edit vulnerable package";
            s.Description = "Edit vulnerable package";
            s.ExampleRequest = new EditVulnerablePackageRequest(
                VulnerablePackageId: -1, 
                IsFalsePositive: true, 
                IsPatchless: true);
        });
        Description(x => x
            .WithTags("Admin")
            .Accepts<EditVulnerablePackageRequest>("application/json")
            .Produces<ProblemDetails>(404, "application/problem+json"));
    }
    
    public override async Task<Results<Ok<EditVulnerablePackageResponse>, ProblemDetails>> ExecuteAsync(
        EditVulnerablePackageRequest req,
        CancellationToken ct)
    {
        var vulnerablePackage = await packageRepository.GetVulnerableByIdAsync(req.VulnerablePackageId, ct);
        if (vulnerablePackage is null)
        {
            AddError(x => x.VulnerablePackageId, $"Vulnerable package {req.VulnerablePackageId} not found");
            return new ProblemDetails(ValidationFailures, (int) HttpStatusCode.NotFound);
        }
        
        if (req.IsFalsePositive is not null)
            vulnerablePackage.IsFalsePositive = req.IsFalsePositive.Value;
        
        if (req.IsPatchless is not null)
            vulnerablePackage.IsPatchless = req.IsPatchless.Value;
        
        await unitOfWork.SaveChangesAsync(ct);
        
        return TypedResults.Ok(vulnerablePackage.MapToResponse());
    }
}