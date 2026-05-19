using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Memory;
using ScanVul.Contracts.PackageInfos;
using ScanVul.Server.Application.Services;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.AgentAggregate.Services;
using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Application.Features.Agents.PackageInfos.ReportPackages;

public class ReportPackagesEndpoint(
    IAgentRepository agentRepository,
    IPackageInfoRepository packageInfoRepository,
    ScannerJobDispatcher scannerJobDispatcher,
    IMemoryCache cache,
    IUnitOfWork unitOfWork) 
    : Endpoint<ReportPackagesRequestWrapper, Results<Ok, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Post("api/{apiVersion}/agents/packages/report");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Report agent's computer packages";
            s.Description = "Report agent's computer packages";
            s.ExampleRequest = new ReportPackagesRequestWrapper
            {
                AgentToken = Guid.Empty,
                Packages = [
                    new PackageInfoDto("7-Zip", "24.09"),
                    new PackageInfoDto("firefox", "145.0.1-1"),
                ]
            };
        });
        Description(x => x.WithTags("Agents"));
    }
    
    public override async Task<Results<Ok, ProblemDetails>> ExecuteAsync(
        ReportPackagesRequestWrapper req, 
        CancellationToken ct)
    {
        var agent = await agentRepository.GetByTokenWithComputerPackagesAsync(req.AgentToken, ct);
        if (agent == null)
        {
            AddError(x => x.AgentToken, "Agent not found");
            return new ProblemDetails(ValidationFailures, statusCode: (int) HttpStatusCode.Unauthorized);
        }
        
        var incomingDtos = req.Packages
            .Select(p => new { 
                Name = p.Name.Trim().ToLowerInvariant(),
                Version = p.Version.Trim().ToLowerInvariant()
            })
            .DistinctBy(x => x.Name)
            .ToList();
        
        var currentlyLinkedPackages = agent.Computer.Packages;
        
        // remove
        var packagesToUnlink = currentlyLinkedPackages
            .Where(existing => !incomingDtos.Any(inc => 
                inc.Name == existing.Name && 
                inc.Version == existing.Version))
            .ToList();
        foreach (var pkg in packagesToUnlink)
        {
            currentlyLinkedPackages.Remove(pkg);
        }

        // add
        var newLinksNeeded = incomingDtos
            .Where(inc => !currentlyLinkedPackages.Any(existing => 
                existing.Name == inc.Name && 
                existing.Version == inc.Version))
            .ToList();

        List<PackageInfo> packagesToLink = [];
        if (newLinksNeeded.Count != 0)
        {
            var namesToLookUp = newLinksNeeded.Select(x => x.Name).ToList();
            
            var potentialMatches = await packageInfoRepository.GetAsync(
                x => namesToLookUp.Contains(x.Name), ct);

            foreach (var dto in newLinksNeeded)
            {
                var existingEntity = potentialMatches
                    .FirstOrDefault(p => p.Name == dto.Name && p.Version == dto.Version);

                if (existingEntity != null)
                {
                    packagesToLink.Add(existingEntity);
                }
                else
                {
                    var newEntity = new PackageInfo(dto.Name, dto.Version);
                    packagesToLink.Add(newEntity);
                }
            }
        }
        
        foreach (var pkg in packagesToLink)
        {
            currentlyLinkedPackages.Add(pkg);
        }
        
        agent.LastPackagesScrapingAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(ct);
        
        await SavePackagesChangesAsync(agent.Computer.Id, packagesToUnlink, packagesToLink, ct);
        scannerJobDispatcher.DispatchScan(agent.Computer.Id);
        
        return TypedResults.Ok();
    }
    
    /// <summary>
    /// Save changes of vulnerable packages (in cache) to use them later in <see cref="IScanSnapshotGenerator"/>
    /// </summary>
    /// <param name="computerId">Computer ID</param>
    /// <param name="removedPackages">Removed ones</param>
    /// <param name="addedPackages">Added ones</param>
    /// <param name="ct">Cancellation token</param>
    private Task SavePackagesChangesAsync(
        long computerId,
        List<PackageInfo> removedPackages,
        List<PackageInfo> addedPackages,
        CancellationToken ct = default)
    {
        cache.Set(CacheKeys.AddedPackages(computerId), addedPackages);
        cache.Set(CacheKeys.RemovedPackages(computerId), removedPackages);
        
        return Task.CompletedTask;
    }
}