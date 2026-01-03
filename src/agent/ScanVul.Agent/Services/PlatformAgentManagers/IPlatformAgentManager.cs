namespace ScanVul.Agent.Services.PlatformAgentManagers;

public interface IPlatformAgentManager
{
    Task DisableAgentAsync(CancellationToken ct = default);
}