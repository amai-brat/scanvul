using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Entities.Commands;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Application.Features.Admin.Agents.Commands.DisableAgentCommand;

public class DisableAgentCommandEndpoint(
    IAgentRepository agentRepository,
    IUnitOfWork unitOfWork) 
    : Endpoint<DisableAgentCommandRequest, Results<Ok, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Post("api/{apiVersion}/admin/agents/{agentId}/commands/disable-agent");
        Summary(s =>
        {
            s.Summary = "Send to agent 'disable agent' command";
            s.Description = "Send to agent 'disable agent' command. " +
                            "Agent will be stopped and removed from services. " +
                            "On database flag 'IsActive' will be set to false.";
        });
        Description(x => x
            .WithTags("Admin")
            .Accepts<DisableAgentCommandRequest>());
    }

    public override async Task<Results<Ok, ProblemDetails>> ExecuteAsync(
        DisableAgentCommandRequest req,
        CancellationToken ct)
    {
        var agent = await agentRepository.GetWithCommandsAsync(req.AgentId, ct);
        if (agent == null)
        {
            AddError(x => x.AgentId, "Agent not found");
            return new ProblemDetails(ValidationFailures, statusCode: (int) HttpStatusCode.NotFound);
        }

        var command = new AgentCommand(agent, new DisableAgentCommandBody());
        agent.Commands.Add(command);
        
        agent.IsActive = false;
        
        await unitOfWork.SaveChangesAsync(ct);
        
        return TypedResults.Ok();
    }
}