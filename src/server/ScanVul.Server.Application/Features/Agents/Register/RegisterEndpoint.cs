using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using ScanVul.Contracts;
using ScanVul.Contracts.Agents;
using ScanVul.Server.Application.Helpers;
using ScanVul.Server.Domain.Entities;
using ScanVul.Server.Domain.Repositories;

namespace ScanVul.Server.Application.Features.Agents.Register;

public class RegisterEndpoint(
    ILogger<RegisterEndpoint> logger,
    IAgentRepository agentRepository,
    IUnitOfWork unitOfWork) 
    : Endpoint<RegisterAgentRequest, Results<Ok<RegisterAgentResponse>, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Post("api/{apiVersion}/agents/register");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Register agent";
            s.Description = "Register agent and return token that will be used to identify agent";
            s.ExampleRequest = new RegisterAgentRequest("Windows 10", "22H2");
        });
        Description(x => x.WithTags("Agents"));
    }

    public override async Task<Results<Ok<RegisterAgentResponse>, ProblemDetails>> ExecuteAsync(
        RegisterAgentRequest req, 
        CancellationToken ct)
    {
        if (HttpContext.Connection.RemoteIpAddress != null)
            logger.LogInformation("Request to register agent from {IpEndpoint}", new IPEndPoint(HttpContext.Connection.RemoteIpAddress, HttpContext.Connection.RemotePort));
        
        var computer = new Computer(HttpContext.Connection.RemoteIpAddress!)
        {
            OperatingSystem = OperatingSystemClassifier.Classify(
                req.OperatingSystemName, 
                req.OperatingSystemVersion)
        };

        var agent = new Agent(computer)
        {
            Token = Guid.CreateVersion7()
        };
        await agentRepository.AddAsync(agent, ct);

        await unitOfWork.SaveChangesAsync(ct);
        
        return TypedResults.Ok(new RegisterAgentResponse(agent.Token));
    }
}