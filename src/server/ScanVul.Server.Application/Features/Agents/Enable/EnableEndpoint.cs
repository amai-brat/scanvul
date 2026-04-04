using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Contracts.Agents;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Application.Features.Agents.Enable;

public class EnableEndpoint(
    IAgentRepository agentRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<EnableAgentRequest, Results<Ok, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Post("api/{apiVersion}/agents/enable");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Enable agent";
            s.Description = "Enable agent that was disabled earlier";
            s.ExampleRequest = new EnableAgentRequest(Guid.Empty);
        });
        Description(x => x.WithTags("Agents"));
    }

    public override async Task<Results<Ok, ProblemDetails>> ExecuteAsync(
        EnableAgentRequest req,
        CancellationToken ct)
    {
        var agent = await agentRepository.GetByTokenWithComputerAsync(req.AgentToken, ct);
        if (agent == null)
        {
            AddError(x => x.AgentToken, "Agent not found");
            return new ProblemDetails(ValidationFailures, statusCode: (int)HttpStatusCode.Unauthorized);
        }

        agent.IsActive = true;

        await unitOfWork.SaveChangesAsync(ct);

        return TypedResults.Ok();
    }
}