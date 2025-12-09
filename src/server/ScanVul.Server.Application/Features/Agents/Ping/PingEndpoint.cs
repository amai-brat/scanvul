using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Application.Features.Agents.Ping;

public class PingEndpoint(
    IAgentRepository agentRepository,
    IUnitOfWork unitOfWork) 
    : Endpoint<PingRequest, Results<Ok, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Post("api/{apiVersion}/agents/ping");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Ping server";
            s.Description = "Ping server for healthcheck";
            s.ExampleRequest = new PingRequest
            {
                AgentToken = Guid.Empty
            };
        });
        Description(x => x.WithTags("Agents"));
    }

    public override async Task<Results<Ok, ProblemDetails>> ExecuteAsync(
        PingRequest req,
        CancellationToken ct)
    {
        var agent = await agentRepository.GetByTokenWithComputerAsync(req.AgentToken, ct);
        if (agent == null)
        {
            AddError(x => x.AgentToken, "Agent not found");
            return new ProblemDetails(ValidationFailures, statusCode: (int) HttpStatusCode.Unauthorized);
        }

        agent.LastPingAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(ct);
        
        return TypedResults.Ok();
    }
}