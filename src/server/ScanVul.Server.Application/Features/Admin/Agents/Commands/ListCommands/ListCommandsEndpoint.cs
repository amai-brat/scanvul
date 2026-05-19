using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Application.Helpers;
using ScanVul.Server.Domain.AgentAggregate.Repositories;

namespace ScanVul.Server.Application.Features.Admin.Agents.Commands.ListCommands;

public class ListCommandsEndpoint(
    IAgentRepository agentRepository)
    : Endpoint<ListCommandsRequest, Results<Ok<ListCommandsResponse>, ProblemDetails>>
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
    
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
    
    public override async Task HandleAsync(ListCommandsRequest req, CancellationToken ct)
    {
        var agent = await agentRepository.GetWithCommandsNoTrackingAsync(req.AgentId, ct);
        if (agent == null)
        {
            AddError(x => x.AgentId, "Agent not found");
            await Send.ResultAsync(new ProblemDetails(ValidationFailures, statusCode: (int)HttpStatusCode.NotFound));
            return;
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

        await this.SendCustom(new ListCommandsResponse(dtos), 
            statusCode: 200, serializerOptions: SerializerOptions, ct: ct);
    }
}