using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.EditBduVulnerablePackage;
using ScanVul.Server.Domain.AgentAggregate.Enums;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.BulkEditVulnerablePackages;

public class BulkEditVulnerablePackagesEndpoint(
    IPackageInfoRepository packageRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<
        BulkEditVulnerablePackagesRequest, 
        Results<Ok<BulkEditVulnerablePackagesResponse>, ProblemDetails>
    >
{
    public override void Configure()
    {
        Version(1);
        Patch("api/{apiVersion}/admin/agents/vulnerable-packages");
        Summary(s =>
        {
            s.Summary = "Edit vulnerable packages";
            s.Description = "Edit vulnerable packages";
            s.ExampleRequest = new BulkEditVulnerablePackagesRequest(
                VulnerablePackageIds: [-1, -2], 
                Status: VulnerablePackageStatus.FalsePositive);
        });
        Description(x => x
            .WithTags("Admin")
            .Accepts<EditBduVulnerablePackageRequest>("application/json")
            .Produces<ProblemDetails>(404, "application/problem+json"));
    }
        
    public override async Task<Results<Ok<BulkEditVulnerablePackagesResponse>, ProblemDetails>> ExecuteAsync(
        BulkEditVulnerablePackagesRequest req,
        CancellationToken ct)
    {
        var vulnerablePackages = await packageRepository.GetVulnerableByIdsAsync(req.VulnerablePackageIds, ct);
        foreach (var vulnerablePackage in vulnerablePackages)
        {
            if (req.Status is not null)
                vulnerablePackage.Status = req.Status.Value;
        }
            
        await unitOfWork.SaveChangesAsync(ct);
            
        return TypedResults.Ok(new BulkEditVulnerablePackagesResponse());
    }
}