using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Agents.PackageInfos.ListPackages;

public class ListPackagesRequest
{
    [FromRoute(Name = "agentId")]
    public long AgentId { get; set; }
}