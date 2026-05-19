using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace ScanVul.Agent.Services.PackageManagers;

public class ChocoPackageManager(
    ILogger<ChocoPackageManager> logger) : IPackageManager
{
    private static string GetVersionArguments(string? packageVersion)
    {
        if (string.IsNullOrWhiteSpace(packageVersion)) return string.Empty;

        return $"--version {packageVersion} --allow-downgrade";
    }
    
    public async Task UpgradePackageAsync(string packageName, string? packageVersion, CancellationToken ct = default)
    {
        using var runspace = RunspaceFactory.CreateRunspace();
        // ReSharper disable once MethodHasAsyncOverload
        runspace.Open();
    
        using var ps = PowerShell.Create();
        ps.Runspace = runspace;

        ps.AddScript($"choco upgrade {packageName} {GetVersionArguments(packageVersion)} -y --fail-on-unfound 2>&1; $LASTEXITCODE");
        
        var results = await ps.InvokeAsync();
        var lastResult = results.LastOrDefault();
        var exitCode = 0;

        if (lastResult != null && int.TryParse(lastResult.ToString(), out var code))
        {
            exitCode = code;
            results.RemoveAt(results.Count - 1); 
        }

        var chocoMessage = string.Join("\n", results);
        if (exitCode != 0)
        {
            logger.LogWarning("Choco failed with exit code {ChocoExitCode} : {ChocoMessage}", exitCode, chocoMessage);
            throw new Exception($"Choco upgrade failed with code {exitCode} : {chocoMessage}");
        }
        
        logger.LogInformation("Choco upgrade successful: {ChocoMessage}", chocoMessage);
        
        if (ps.Streams.Error.Count > 0)
            throw new AggregateException($"Error when upgrading package {packageName}", 
                ps.Streams.Error.Select(x => x.Exception));
    }
}