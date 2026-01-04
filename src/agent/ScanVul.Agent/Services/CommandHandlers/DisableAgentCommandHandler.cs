using ScanVul.Agent.Services.PlatformAgentManagers;
using ScanVul.Contracts.Agents;

namespace ScanVul.Agent.Services.CommandHandlers;

public class DisableAgentCommandHandler(
    ILogger<ReportPackagesCommandHandler> logger,
    IPlatformAgentManager agentManager) : ICommandHandler<DisableAgentCommand>
{
    public async Task<string> Handle(DisableAgentCommand command, CancellationToken ct = default)
    {
        logger.LogInformation("Processing {Command}:{CommandId}", command.GetType().Name, command.CommandId);
        
        try
        {
            await agentManager.DisableAgentAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error when disabling agent");
            return $"Error when disabling agent: {ex.Message}";
        }

        return "OK";
    }
}