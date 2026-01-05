using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Application.Helpers;
using ScanVul.Server.Domain.PackageManagers.Services;

namespace ScanVul.Server.Application.Features.Admin.PackageManagers.SearchPackage;

public class SearchPackageEndpoint
    : Endpoint<SearchPackageRequest, Results<Ok<SearchPackageResponse>, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Get("api/{apiVersion}/admin/package-managers/search");
        Summary(s =>
        {
            s.Summary = "Search package from package managers";
            s.Description = "Search package from package managers";
            s.ExampleRequest = new SearchPackageRequest("7-Zip", PackageManagerType.Choco);
        });
        Description(x => x
            .WithTags("Admin")
            .Produces<ProblemDetails>(404, "application/problem+json"));
    }

    public override async Task<Results<Ok<SearchPackageResponse>, ProblemDetails>> ExecuteAsync(
        SearchPackageRequest req,
        CancellationToken ct)
    {
        var packageManager = TryResolve<IPackageManager>(req.PackageManager.ToString().ToLowerInvariant());
        if (packageManager is null)
        {
            AddError(x => x.PackageManager, "Appropriate package manager not found");
            return new ProblemDetails(ValidationFailures, (int)HttpStatusCode.NotFound);
        }

        var results = await packageManager.SearchAsync(req.PackageName, ct);

        return TypedResults.Ok(new SearchPackageResponse(results));
    }
}