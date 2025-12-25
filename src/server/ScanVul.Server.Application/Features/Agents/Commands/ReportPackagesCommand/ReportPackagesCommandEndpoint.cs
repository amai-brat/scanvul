using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Entities.Commands;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Application.Features.Agents.Commands.ReportPackagesCommand;

public class ReportPackagesCommandEndpoint(
    IAgentRepository agentRepository,
    IUnitOfWork unitOfWork) 
    : Endpoint<ReportPackagesCommandRequest, Results<Ok, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Post("api/{apiVersion}/agents/{agentId}/commands/report-packages");
        Summary(s =>
        {
            s.Summary = "Send to agent report packages command";
            s.Description = "Send to agent report packages command";
        });
        Description(x => x
            .WithTags("Agents")
            .Accepts<ReportPackagesCommandRequest>());
    }

    public override async Task<Results<Ok, ProblemDetails>> ExecuteAsync(
        ReportPackagesCommandRequest req,
        CancellationToken ct)
    {
        var agent = await agentRepository.GetWithCommandsAsync(req.AgentId, ct);
        if (agent == null)
        {
            AddError(x => x.AgentId, "Agent not found");
            return new ProblemDetails(ValidationFailures, statusCode: (int) HttpStatusCode.NotFound);
        }

        var command = new AgentCommand(agent, new ReportPackagesCommandBody());
        agent.Commands.Add(command);
        
        await unitOfWork.SaveChangesAsync(ct);
        
        return TypedResults.Ok();
    }
}