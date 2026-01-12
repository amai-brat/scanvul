using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Application.Features.Agents.RespondToCommand;

public class RespondToCommandEndpoint(
    IAgentRepository agentRepository,
    IUnitOfWork unitOfWork) 
    : Endpoint<RespondToCommandRequestWrapper, Results<Ok, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Post("api/{apiVersion}/agents/commands:respond");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Respond to server command";
            s.Description = "Respond to server command (by agent usually)";
            s.ExampleRequest = new RespondToCommandRequestWrapper(Guid.Empty, "OK")
            {
                AgentToken = Guid.Empty
            };
        });
        Description(x => x.WithTags("Agents"));
    }

    public override async Task<Results<Ok, ProblemDetails>> ExecuteAsync(
        RespondToCommandRequestWrapper req,
        CancellationToken ct)
    {
        var agent = await agentRepository.GetByTokenWithCommandAsync(req.AgentToken, req.CommandId, ct);
        if (agent == null)
        {
            AddError(x => x.AgentToken, "Agent not found");
            return new ProblemDetails(ValidationFailures, statusCode: (int) HttpStatusCode.Unauthorized);
        }

        var command = agent.Commands.FirstOrDefault(x => x.Id == req.CommandId);
        if (command == null)
        {
            AddError(x => x.CommandId, "Command not found");
            return new ProblemDetails(ValidationFailures, statusCode: (int) HttpStatusCode.NotFound);

        }

        command.AgentResponse = req.Message;
        
        await unitOfWork.SaveChangesAsync(ct);
        
        return TypedResults.Ok();
    }
}