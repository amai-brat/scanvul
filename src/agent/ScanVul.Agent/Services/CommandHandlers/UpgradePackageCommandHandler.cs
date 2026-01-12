using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;
using ScanVul.Agent.Services.PlatformPackageManagers;
using ScanVul.Contracts.Agents;

namespace ScanVul.Agent.Services.CommandHandlers;

public class UpgradePackageCommandHandler(
    ILogger<ReportPackagesCommandHandler> logger,
    IPlatformPackageManager packageManager) : ICommandHandler<UpgradePackageCommand>
{
    public async Task<string> Handle(UpgradePackageCommand command, CancellationToken ct = default)
    {
        logger.LogInformation("Processing {Command}:{CommandId}", command.GetType().Name, command.CommandId);
        
        try
        {
            await packageManager.UpgradePackageAsync(command.PackageName, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error when upgrading package {PackageName}", command.PackageName);
            return $"Error when upgrading package {command.PackageName}: {ex.ToInvariantString()}";
        }
        
        logger.LogInformation("Successfully upgraded package {PackageName}", command.PackageName);
        return "OK";
    }
}