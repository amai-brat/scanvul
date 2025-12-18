using FastEndpoints;
using ScanVul.Contracts.PackageInfos;

namespace ScanVul.Server.Application.Features.Agents.PackageInfos.ReportPackages;

public class ReportPackagesRequestWrapper : ReportPackagesRequest
{
    [FromHeader("X-Agent-Token")] 
    public Guid AgentToken { get; init; }
}