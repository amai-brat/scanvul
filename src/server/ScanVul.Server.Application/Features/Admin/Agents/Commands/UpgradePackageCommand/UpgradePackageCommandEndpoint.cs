using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Entities.Commands;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Application.Features.Admin.Agents.Commands.UpgradePackageCommand;

public class UpgradePackageCommandEndpoint(
    IAgentRepository agentRepository,
    IUnitOfWork unitOfWork) 
    : Endpoint<UpgradePackageCommandRequest, Results<Ok, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Post("api/{apiVersion}/admin/agents/{agentId}/commands/upgrade-package");
        Summary(s =>
        {
            s.Summary = "Send to agent 'upgrade package' command";
            s.Description = "Send to agent 'upgrade package' command.";
        });
        Description(x => x
            .WithTags("Admin")
            .Accepts<UpgradePackageCommandRequest>("application/json"));
    }

    public override async Task<Results<Ok, ProblemDetails>> ExecuteAsync(
        UpgradePackageCommandRequest req,
        CancellationToken ct)
    {
        var agent = await agentRepository.GetWithCommandsAsync(req.AgentId, ct);
        if (agent == null)
        {
            AddError(x => x.AgentId, "Agent not found");
            return new ProblemDetails(ValidationFailures, statusCode: (int) HttpStatusCode.NotFound);
        }

        var command = new AgentCommand(agent, new UpgradePackageCommandBody(req.PackageName));
        agent.Commands.Add(command);
        
        await unitOfWork.SaveChangesAsync(ct);
        
        return TypedResults.Ok();
    }
}