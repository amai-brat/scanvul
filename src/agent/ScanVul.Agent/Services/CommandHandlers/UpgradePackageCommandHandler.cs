using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;
using ScanVul.Agent.Services.PackageManagers;
using ScanVul.Contracts.Agents;

namespace ScanVul.Agent.Services.CommandHandlers;

public class UpgradePackageCommandHandler(
    ILogger<ReportPackagesCommandHandler> logger,
    Func<string, IPackageManager> packageManagerFactory) : ICommandHandler<UpgradePackageCommand>
{
    public async Task<string> Handle(UpgradePackageCommand command, CancellationToken ct = default)
    {
        logger.LogInformation("Processing {Command}:{CommandId}", command.GetType().Name, command.CommandId);
        
        try
        {
            var packageManager = packageManagerFactory(command.PackageManager);
            
            await packageManager.UpgradePackageAsync(command.PackageName, ct);
        }
        catch (InvalidOperationException) 
        {
            logger.LogWarning("Unknown package manager: {PackageManager}", command.PackageManager);
            return $"Unknown package manager: {command.PackageManager}";
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