using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace ScanVul.Agent.Services.PackageManagers;

public class WingetPackageManager(
    ILogger<WingetPackageManager> logger) : IPackageManager
{
    private const string FindWingetScript = """
        $wingetDir = Get-ChildItem -Path 'C:\Program Files\WindowsApps' -Filter 'Microsoft.DesktopAppInstaller_*_x64__8wekyb3d8bbwe' -ErrorAction SilentlyContinue | 
                     Sort-Object Name -Descending | 
                     Select-Object -First 1;

        if ($wingetDir) {
            return Join-Path $wingetDir.FullName 'winget.exe';
        }
        return $null;
        """;
    
    private const int NoAppsFoundCode = -1978335212;
    private const int NoUpdateAvailableCode = -1978335189;
    
    public async Task UpgradePackageAsync(string packageName, string? packageVersion, CancellationToken ct = default)
    {
        using var runspace = RunspaceFactory.CreateRunspace();
        // ReSharper disable once MethodHasAsyncOverload
        runspace.Open();

        using var ps = PowerShell.Create();
        ps.Runspace = runspace;
        
        var wingetPath = await FindWingetPath(ps);

        var upgradeCommand = new WingetCommandBuilder(wingetPath, "upgrade")
            .WithDefaultParams()
            .WithExactMatch()
            .WithIncludeUnknown()
            .WithId(packageName)
            .Build();
        
        logger.LogInformation("Attempting upgrade for {Package}...", packageName);
        
        var (exitCode, message) = await RunCommandAsync(ps, upgradeCommand);

        switch (exitCode)
        {
            case 0:
                logger.LogInformation("Winget upgrade successful: {Message}", message);
                return;
            case NoUpdateAvailableCode:
                logger.LogInformation("Package {Package} is already up to date.", packageName);
                return;
            case NoAppsFoundCode:
            {
                logger.LogWarning("Package ID '{Package}' not found (It might be an unmanaged 'ARP' entry). Switching to Install/Adopt mode...", packageName);
                
                var installCommand = new WingetCommandBuilder(wingetPath, "install")
                    .WithDefaultParams()
                    .WithExactMatch()
                    .WithIncludeUnknown()
                    .WithId(packageName)
                    .WithVersion(packageVersion)
                    .Build();
                
                (exitCode, message) = await RunCommandAsync(ps, installCommand);
        
                if (exitCode == NoUpdateAvailableCode) 
                {
                    logger.LogInformation("Adoption complete. {Package} is already up to date", packageName);
                    return;
                }

                if (exitCode != 0)
                {
                    logger.LogWarning("Couldn't install {Package}: {Message}", packageName, message);
                    throw new Exception($"{packageName} is not matched. Tried to install, but got error with code {exitCode}: {message}");
                }
                
                logger.LogInformation("Adoption complete. Installed {Package}", packageName);
                break;
            }
            default:
                logger.LogError("Winget failed with exit code {Code}: {Message}", exitCode, message);
                throw new Exception($"Winget upgrade failed with code {exitCode}: {message}");
        }
    }

    private async Task<string> FindWingetPath(PowerShell ps)
    {
        ps.AddScript(FindWingetScript);
        var pathResult = await ps.InvokeAsync();
        var wingetPath = pathResult.FirstOrDefault()?.ToString();

        if (string.IsNullOrEmpty(wingetPath))
        {
            logger.LogError("Winget executable not found. It may not be installed on this system.");
            throw new FileNotFoundException("Could not locate winget.exe in WindowsApps.");
        }
        
        logger.LogInformation("Located Winget at: {Path}", wingetPath);
        ps.Commands.Clear();

        return wingetPath;
    }

    private async Task<(int ExitCode, string Message)> RunCommandAsync(PowerShell ps, string script)
    {
        logger.LogInformation("Executing Winget command: {Command}", script);
        
        ps.AddScript(script);
        var results = await ps.InvokeAsync();
        
        var lastResult = results.LastOrDefault();
        var exitCode = 0;
        
        if (lastResult != null && int.TryParse(lastResult.ToString(), out var code))
        {
            exitCode = code;
            if (results.Count > 0) results.RemoveAt(results.Count - 1);
        }
        else if (ps.Streams.Error.Count > 0)
        {
            exitCode = 1;
        }

        var message = string.Join(Environment.NewLine, results);
        return (exitCode, message);
    }
}