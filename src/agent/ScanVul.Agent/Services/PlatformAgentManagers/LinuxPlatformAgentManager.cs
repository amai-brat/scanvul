using System.Diagnostics;
using System.Runtime.Versioning;

namespace ScanVul.Agent.Services.PlatformAgentManagers;

[SupportedOSPlatform("linux")]
public class LinuxPlatformAgentManager : IPlatformAgentManager
{
    private const string SystemdUnitFileName = "scanvul-agent.service";

    public async Task DisableAgentAsync(CancellationToken ct = default)
    {
        // 1. Disable and Stop the service immediately
        // 'disable' removes auto-start symlinks. '--now' also stops it immediately
        await RunSystemCommandAsync("systemctl", $"disable --now {SystemdUnitFileName}", ct);
    }
    
    private static async Task RunSystemCommandAsync(string command, string arguments, CancellationToken ct = default)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processStartInfo);
        if (process == null)
        {
            throw new InvalidOperationException($"Failed to start process: {command}");
        }

        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(ct);
            throw new InvalidOperationException($"Command '{command} {arguments}' failed with exit code {process.ExitCode}: {error}");
        }
    }
}