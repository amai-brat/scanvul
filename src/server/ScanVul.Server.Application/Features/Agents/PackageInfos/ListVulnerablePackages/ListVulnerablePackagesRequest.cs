using Microsoft.AspNetCore.Mvc;

namespace ScanVul.Server.Application.Features.Agents.PackageInfos.ListVulnerablePackages;

public class ListVulnerablePackagesRequest
{
    [FromRoute(Name = "agentId")]
    public long AgentId { get; set; }
}