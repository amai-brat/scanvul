using ScanVul.Contracts.Agents;
using ScanVul.Server.Domain.AgentAggregate.Entities.Commands;
using AgentCommand = ScanVul.Server.Domain.AgentAggregate.Entities.Commands.AgentCommand;

namespace ScanVul.Server.Application.Features.Agents.Ping;

public static class Mapping
{
    public static Contracts.Agents.AgentCommand MapToResponse(this AgentCommand command)
    {
        return command.Body switch
        {
            ReportPackagesCommandBody => new ReportPackagesCommand(command.Id),
            UpgradePackageCommandBody upgradePackageCommandBody => new UpgradePackageCommand(command.Id, upgradePackageCommandBody.PackageName),
            _ => throw new ArgumentOutOfRangeException(nameof(command), command, null)
        };
    }
}