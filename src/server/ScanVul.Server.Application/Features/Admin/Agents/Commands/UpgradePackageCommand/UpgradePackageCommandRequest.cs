using Microsoft.AspNetCore.Mvc;
using ScanVul.Server.Application.Helpers;

namespace ScanVul.Server.Application.Features.Admin.Agents.Commands.UpgradePackageCommand;

/// <summary>
/// Request to 'upgrade package' command to agent
/// </summary>
/// <param name="AgentId">Agent ID</param>
/// <param name="PackageVersion">Version to upgrade (will be used if package manager can handle it, unless latest version will be used)</param>
/// <param name="PackageName">Exact package name from package manager</param>
/// <param name="PackageManager">Package manager</param>
public record UpgradePackageCommandRequest(
    [FromRoute(Name = "agentId")] long AgentId,
    string PackageName,
    string? PackageVersion,
    PackageManagerType PackageManager);