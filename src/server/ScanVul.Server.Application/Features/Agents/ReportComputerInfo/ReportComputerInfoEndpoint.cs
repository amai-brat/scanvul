using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Application.Features.Agents.ReportComputerInfo;

public class ReportComputerInfoEndpoint(
    IAgentRepository agentRepository,
    IUnitOfWork unitOfWork) 
    : Endpoint<ReportComputerInfoRequestWrapper, Results<Ok, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Post("api/{apiVersion}/agents/computer/report");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Report computer info";
            s.Description = "Report computer info";
            s.ExampleRequest = new ReportComputerInfoRequestWrapper
            {
                AgentToken = Guid.Empty,
                ComputerName = "pepega",
                MemoryInMb = 32768,
                CpuName = "AMD Ryzen 7 8745HS",
            };
        });
        Description(x => x.WithTags("Agents"));
    }

    public override async Task<Results<Ok, ProblemDetails>> ExecuteAsync(
        ReportComputerInfoRequestWrapper req,
        CancellationToken ct)
    {
        var agent = await agentRepository.GetByTokenWithComputerAsync(req.AgentToken, ct);
        if (agent == null)
        {
            AddError(x => x.AgentToken, "Agent not found");
            return new ProblemDetails(ValidationFailures, statusCode: (int) HttpStatusCode.Unauthorized);
        }

        agent.Computer.Name = req.ComputerName.Trim();
        agent.Computer.MemoryInMb = req.MemoryInMb;
        agent.Computer.CpuName = req.CpuName.Trim();
        
        await unitOfWork.SaveChangesAsync(ct);
        
        return TypedResults.Ok();
    }
}