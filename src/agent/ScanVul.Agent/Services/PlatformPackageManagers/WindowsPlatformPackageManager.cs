using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace ScanVul.Agent.Services.PlatformPackageManagers;

public class WindowsPlatformPackageManager(
    ILogger<WindowsPlatformPackageManager> logger) : IPlatformPackageManager
{
    public async Task UpgradePackageAsync(string packageName, CancellationToken ct = default)
    {
        try
        {
            using var runspace = RunspaceFactory.CreateRunspace();
            // ReSharper disable once MethodHasAsyncOverload
            runspace.Open();
        
            using var ps = PowerShell.Create();
            ps.Runspace = runspace;

            ps.AddScript($"choco upgrade {packageName}");

            await ps.InvokeAsync();

            foreach (var record in ps.Streams.Information)
                logger.LogInformation("Choco log: {ChocoMessage}", record.MessageData);
            
            foreach (var record in ps.Streams.Warning)
                logger.LogWarning("Choco log: {ChocoMessage}", record.Message);

            if (ps.Streams.Error.Count > 0)
                throw new AggregateException($"Error when upgrading package {packageName}", 
                    ps.Streams.Error.Select(x => x.Exception));
        }
        catch (Exception ex)
        {
            throw new AggregateException($"Error when upgrading package {packageName}", ex);
        }
    }
}