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

        ps.AddStatement()
            .AddCommand("Remove-Service")
            .AddParameter("Name", ServiceName);

        await ps.InvokeAsync();

        if (ps.Streams.Error.Count > 0)
        {
            var exceptions = ps.Streams.Error.Select(x => x.Exception).ToList();
            throw new AggregateException("Error executing PowerShell commands for service removal.", exceptions);
        }
    }
}