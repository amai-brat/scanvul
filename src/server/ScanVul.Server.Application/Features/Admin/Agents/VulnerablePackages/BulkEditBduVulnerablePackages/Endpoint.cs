using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.EditBduVulnerablePackage;
using ScanVul.Server.Domain.AgentAggregate.Enums;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Application.Features.Admin.Agents.VulnerablePackages.BulkEditBduVulnerablePackages;

public class BulkEditVulnerablePackagesEndpoint(
    IPackageInfoRepository packageRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<
        BulkEditBduVulnerablePackagesRequest, 
        Results<Ok<BulkEditBduVulnerablePackagesResponse>, ProblemDetails>
    >
{
    public override void Configure()
    {
        Version(1);
        Patch("api/{apiVersion}/admin/agents/bdu-vulnerable-packages");
        Summary(s =>
        {
            s.Summary = "Edit BDU vulnerable packages";
            s.Description = "Edit BDU vulnerable packages";
            s.ExampleRequest = new BulkEditBduVulnerablePackagesRequest(
                VulnerablePackageIds: [-1, -2], 
                Status: VulnerablePackageStatus.FalsePositive);
        });
        Description(x => x
            .WithTags("Admin")
            .Accepts<EditBduVulnerablePackageRequest>("application/json")
            .Produces<ProblemDetails>(404, "application/problem+json"));
    }
        
    public override async Task<Results<Ok<BulkEditBduVulnerablePackagesResponse>, ProblemDetails>> ExecuteAsync(
        BulkEditBduVulnerablePackagesRequest req,
        CancellationToken ct)
    {
        var vulnerablePackages = await packageRepository.GetBduVulnerableByIdsAsync(req.VulnerablePackageIds, ct);
        foreach (var vulnerablePackage in vulnerablePackages)
        {
            if (req.Status is not null)
                vulnerablePackage.Status = req.Status.Value;
        }
            
        await unitOfWork.SaveChangesAsync(ct);
            
        return TypedResults.Ok(new BulkEditBduVulnerablePackagesResponse());
    }
}