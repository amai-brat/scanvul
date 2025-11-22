using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using ScanVul.Server.Application.Helpers;
using ScanVul.Server.Domain.Entities;
using OperatingSystem = ScanVul.Server.Domain.Entities.OperatingSystem;

namespace ScanVul.Server.Application.Features.Agents.Register;

public class RegisterEndpoint(
    ILogger<RegisterEndpoint> logger) 
    : Endpoint<RegisterRequest, Results<Ok<RegisterResponse>, ProblemDetails>>
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
            s.ExampleRequest = new RegisterRequest("Windows 10", "22H2");
        });
        Description(x => x.WithTags("Agents"));
    }

    public override async Task<Results<Ok<RegisterResponse>, ProblemDetails>> ExecuteAsync(
        RegisterRequest req, 
        CancellationToken ct)
    {
        await Task.CompletedTask;

        var computer = new Computer(HttpContext.Connection.RemoteIpAddress!)
        {
            OperatingSystem = OperatingSystemClassifier.Classify(req.OperatingSystemName, req.OperatingSystemVersion)
        };

        var agent = new Agent(computer);
        
        if (HttpContext.Connection.RemoteIpAddress != null)
            logger.LogInformation("Request from {IpEndpoint}", new IPEndPoint(HttpContext.Connection.RemoteIpAddress, HttpContext.Connection.RemotePort));
        return TypedResults.Ok(new RegisterResponse(Guid.CreateVersion7().ToString()));
    }
}