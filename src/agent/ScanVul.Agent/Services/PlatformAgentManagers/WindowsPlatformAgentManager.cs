using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.Versioning;

namespace ScanVul.Agent.Services.PlatformAgentManagers;

[SupportedOSPlatform("windows")]
public class WindowsPlatformAgentManager : IPlatformAgentManager
{
    private const string ServiceName = "ScanVul.Agent";
    
    public async Task DisableAgentAsync(CancellationToken ct = default)
    {
        using var runspace = RunspaceFactory.CreateRunspace();
        // ReSharper disable once MethodHasAsyncOverload
        runspace.Open();

        using var ps = PowerShell.Create();
        ps.Runspace = runspace;

        // 1. Remove-Service: Marks the service for deletion. 
        // It won't actually disappear until the process stops, but it prevents restarts.
        ps.AddStatement()
            .AddCommand("Remove-Service")
            .AddParameter("Name", ServiceName);

        // 2. Stop-Service: Tells SCM to stop this service.
        // Note: Since we are running INSIDE the service, this command might
        // cause the application to exit immediately or throw an error as the 
        // connection is severed. We place it last.
        ps.AddStatement()
            .AddCommand("Stop-Service")
            .AddParameter("Name", ServiceName)
            .AddParameter("Force"); // Force ensures it tries to kill even if dependent services exist

        await ps.InvokeAsync();

        if (ps.Streams.Error.Count > 0)
        {
            var exceptions = ps.Streams.Error.Select(x => x.Exception).ToList();
            throw new AggregateException("Error executing PowerShell commands for service removal.", exceptions);
        }
    }
}