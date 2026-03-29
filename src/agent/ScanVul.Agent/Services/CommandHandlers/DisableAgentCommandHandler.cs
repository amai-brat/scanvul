using ScanVul.Agent.Services.PlatformAgentManagers;
using ScanVul.Contracts.Agents;

namespace ScanVul.Agent.Services.CommandHandlers;

public class DisableAgentCommandHandler(
    ILogger<ReportPackagesCommandHandler> logger,
    IPlatformAgentManager agentManager,
    IHostApplicationLifetime appLifetime) : ICommandHandler<DisableAgentCommand>
{
    private static readonly TimeSpan ShutdownDelay = TimeSpan.FromSeconds(10);
    public Task<string> Handle(DisableAgentCommand command, CancellationToken ct = default)
    {
        logger.LogInformation("Processing {Command}:{CommandId}", command.GetType().Name, command.CommandId);
        
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(ShutdownDelay, CancellationToken.None);

                logger.LogInformation("Initiating agent uninstallation...");
                await agentManager.DisableAgentAsync(CancellationToken.None);
                
                logger.LogInformation("Agent unregistered. Initiating graceful shutdown...");
                appLifetime.StopApplication(); 
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "FATAL ERROR: Failed to disable agent in background task.");
            }
        }, ct);
       
        logger.LogInformation("Disable sequence scheduled. Returning OK to server.");
        return Task.FromResult("OK");
    }
}