using System.Diagnostics;

namespace ScanVul.Agent.Services.PackageManagers;

/// <summary>
/// RPM Package Manager implementation specifically for Alt Linux (uses apt-get)
/// </summary>
public class RpmPackageManager(ILogger<RpmPackageManager> logger) : IPackageManager
{
    public async Task UpgradePackageAsync(string packageName, string? packageVersion, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageName);
        
        logger.LogInformation("Updating APT repository metadata...");
        await RunAptCommandAsync(["update"], ct);

        logger.LogInformation("Upgrading/Installing package: {Package}", packageName);
        
        await RunAptCommandAsync(["install", "-y", packageName], ct);
        
        logger.LogInformation("Successfully processed package {Package}", packageName);
    }

    private async Task RunAptCommandAsync(List<string> args, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "apt-get",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        using var process = new Process();
        process.StartInfo = psi;

        if (!process.Start())
        {
            throw new InvalidOperationException($"Failed to start apt-get with args: {string.Join(" ", args)}");
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = process.StandardError.ReadToEndAsync(ct);

        try
        {
            await process.WaitForExitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // APT lock files (/var/lib/apt/lists/lock) can be tricky if killed abruptly.
            // Attempt to kill gracefully if possible, otherwise force kill.
            process.Kill(); 
            throw;
        }

        var output = await stdoutTask;
        var error = await stderrTask;

        if (process.ExitCode != 0)
        {
            logger.LogError("APT command failed (Exit Code: {Code}). Error: {Error}", process.ExitCode, error);
            throw new Exception($"APT operation failed: {error}");
        }

        logger.LogDebug("APT Output: {Output}", output);
    }
}