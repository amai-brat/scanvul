using FastEndpoints;
using ScanVul.Contracts.ComputerInfos;

namespace ScanVul.Server.Application.Features.Agents.ReportComputerInfo;

public class ReportComputerInfoRequestWrapper : ReportComputerInfoRequest
{
    [FromHeader("X-Agent-Token")] 
    public Guid AgentToken { get; init; }
}