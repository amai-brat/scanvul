using System.Diagnostics;
using System.Runtime.Versioning;

namespace ScanVul.Agent.Installer.PlatformInstallers;

[SupportedOSPlatform("linux")]
public class LinuxInstaller : IPlatformInstaller
{
    private const string SystemdUnitFileName = "scanvul-agent.service";
    private const string SystemdUnitFilePath = $"/etc/systemd/system/{SystemdUnitFileName}";
    private const string SystemdUnitTemplate = """
                                               [Unit]
                                               Description=ScanVul Agent

                                               [Service]
                                               Type=notify
                                               ExecStart={0}

                                               [Install]
                                               WantedBy=multi-user.target
                                               """;
    private const string OsReleasePath = "/etc/os-release";
    
    public DirectoryInfo DefaultInstallationPath => new("/opt/scanvul");
    public string AgentZipResourceName => "agent.linux.zip";
    public string ExecutableFileName => "ScanVul.Agent";
    public async Task<Result> PrepareInstallationAsync(CancellationToken ct = default)
    {
        try
        {
            var stopResult = await RunSystemCommandAsync("systemctl", $"stop {SystemdUnitFileName}", ct);
            if (stopResult.IsFailure)
            {
                // ignore if not found (first installation)
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("Error when preparing installation", ex);
        }
    }

    public async Task<Result> AddAgentToAutoStartAsync(DirectoryInfo path, CancellationToken ct = default)
    {
        try
        {
            var executablePath = Path.Combine(path.FullName, ExecutableFileName);
            var unitFileContent = string.Format(SystemdUnitTemplate, executablePath);
            
            if (!File.Exists(executablePath))
            {
                return Result.Failure($"Executable not found at {executablePath}");
            }

            await File.WriteAllTextAsync(SystemdUnitFilePath, unitFileContent, ct);
            var reloadResult = await RunSystemCommandAsync("systemctl", "daemon-reload", ct);
            if (reloadResult.IsFailure)
            {
                return reloadResult;
            }
            
            var enableResult = await RunSystemCommandAsync("systemctl", $"enable --now {SystemdUnitFileName}", ct);
            if (enableResult.IsFailure)
            {
                return enableResult;
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("Error when adding agent to services", ex);
        }
    }
    public async Task<Result<string>> GetOsNameAsync(CancellationToken ct = default)
    {
        try
        {
            var values = await ParseOsReleaseAsync(ct);
            if (values.TryGetValue("NAME", out var name) && !string.IsNullOrWhiteSpace(name))
            {
                return Result.Success(name);
            }

            return Result.Failure<string>("NAME key not found in /etc/os-release");
        }
        catch (Exception ex)
        {
            return Result.Failure<string>($"Failed to read Linux OS Name: {ex.Message}");
        }
    }

    public async Task<Result<string?>> GetOsVersionAsync(CancellationToken ct = default)
    {
        try
        {
            var values = await ParseOsReleaseAsync(ct);
            
            return values.TryGetValue("VERSION_ID", out var versionId) 
                ? Result.Success<string?>(versionId) 
                : Result.Success<string?>(null);
        }
        catch (Exception ex)
        {
            return Result.Failure<string?>($"Failed to read Linux OS Version: {ex.Message}");
        }
    }
    
    private static async Task<Result> RunSystemCommandAsync(string command, string arguments, CancellationToken ct = default)
    {
        try
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
                return Result.Failure($"Failed to start process: {command}");
            }

            await process.WaitForExitAsync(ct);

            var output = await process.StandardOutput.ReadToEndAsync(ct);
            var error = await process.StandardError.ReadToEndAsync(ct);

            return process.ExitCode != 0 
                ? Result.Failure($"Command failed with exit code {process.ExitCode}: {error}") 
                : Result.Success(output);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error executing command {command} {arguments}", ex);
        }
    }
    
    private static async Task<Dictionary<string, string>> ParseOsReleaseAsync(CancellationToken ct)
    {
        if (!File.Exists(OsReleasePath))
            throw new FileNotFoundException($"Could not find {OsReleasePath}");

        var lines = await File.ReadAllLinesAsync(OsReleasePath, ct);
        var result = new Dictionary<string, string>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.Contains('=')) 
                continue;

            var parts = line.Split('=', 2);
            var key = parts[0].Trim();
            var value = parts[1].Trim();

            // Remove quotes if present (e.g., "Ubuntu" -> Ubuntu)
            if (value.StartsWith('\"') && value.EndsWith('\"'))
                value = value.Substring(1, value.Length - 2);

            result[key] = value;
        }

        return result;
    }
}