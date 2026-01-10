using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Repositories;

namespace ScanVul.Server.Application.Features.Admin.Agents.Commands.ListCommands;

public class ListCommandsEndpoint(
    IAgentRepository agentRepository)
    : Endpoint<ListCommandsRequest, Results<Ok<ListCommandsResponse>, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Get("api/{apiVersion}/admin/agents/{agentId}/commands");
        Summary(s =>
        {
            s.Summary = "Get all commands of agent";
            s.Description = "Get all commands of agent";
        });
        Description(x => x.WithTags("Admin"));
    }
    
    public override async Task<Results<Ok<ListCommandsResponse>, ProblemDetails>> ExecuteAsync(
        ListCommandsRequest req,
        CancellationToken ct)
    {
        var agent = await agentRepository.GetWithCommandsNoTrackingAsync(req.AgentId, ct);
        if (agent == null)
        {
            AddError(x => x.AgentId, "Agent not found");
            return new ProblemDetails(ValidationFailures, statusCode: (int) HttpStatusCode.NotFound);
        }
        
        var dtos = agent.Commands
            .Select(cmd =>
                new CommandResponse(
                    Id: cmd.Id,
                    Type: cmd.Body.GetType().Name.Replace("CommandBody", string.Empty),
                    CreatedAt: cmd.CreatedAt,
                    SentAt: cmd.SentAt,
                    AgentResponse: cmd.AgentResponse,
                    CommandParams: cmd.Body
                ))
            .ToList();
        
        return TypedResults.Ok(new ListCommandsResponse(dtos));
    }
}