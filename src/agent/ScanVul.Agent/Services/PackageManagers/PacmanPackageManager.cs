using System.Diagnostics;

namespace ScanVul.Agent.Services.PackageManagers;

/// <summary>
/// Pac(kage)man(ager) for Arch Linux
/// </summary>
public class PacmanPackageManager(ILogger<PacmanPackageManager> logger) : IPackageManager
{

    public async Task UpgradePackageAsync(string packageName, string? packageVersion, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageName);

        logger.LogInformation("Initiating system upgrade and update for package: {Package}", packageName);
        
        var psi = new ProcessStartInfo
        {
            FileName = "pacman",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        psi.ArgumentList.Add("-Syu");
        psi.ArgumentList.Add("--noconfirm");
        psi.ArgumentList.Add(packageName);

        using var process = new Process();
        process.StartInfo = psi;

        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start pacman process.");
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = process.StandardError.ReadToEndAsync(ct);

        try
        {
            await process.WaitForExitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            process.Kill();
            throw;
        }

        var output = await stdoutTask;
        var error = await stderrTask;

        if (process.ExitCode != 0)
        {
            logger.LogError("Pacman failed with exit code {Code}. Error: {Error}", process.ExitCode, error);
            throw new Exception($"Pacman upgrade failed: {error}");
        }

        logger.LogInformation("Successfully upgraded system and package {Package}. Output: {Output}", packageName, output);
    }
}