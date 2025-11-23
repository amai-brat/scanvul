using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace ScanVul.Agent.Installer;

public interface IPlatformInstaller
{ 
    DirectoryInfo DefaultInstallationPath { get; }
    string AgentZipResourceName { get; }
    string ExecutableFileName { get; }

    /// <summary>
    /// Add agent to autostart
    /// </summary>
    /// <param name="path">Agent installation path</param>
    /// <param name="ct"></param>
    Task<Result> AddAgentToAutoStartAsync(DirectoryInfo path, CancellationToken ct = default);

    Task<Result<string>> GetOsNameAsync(CancellationToken ct = default);
    Task<Result<string?>> GetOsVersionAsync(CancellationToken ct = default);
}

[SupportedOSPlatform("windows")]
public class WindowsInstaller : IPlatformInstaller
{
    private const string ServiceName = "ScanVul.Agent";
    private const string ServiceDisplayName = "ScanVul Agent";
    
    public DirectoryInfo DefaultInstallationPath => new(@"C:\Program Files\ScanVul");
    public string AgentZipResourceName => "agent.win64.zip";
    public string ExecutableFileName => "ScanVul.Agent.exe";
    public async Task<Result> AddAgentToAutoStartAsync(DirectoryInfo path, CancellationToken ct = default)
    {
        try
        {
            using var runspace = RunspaceFactory.CreateRunspace();
            // ReSharper disable once MethodHasAsyncOverload
            runspace.Open();
        
            using var ps = PowerShell.Create();
            ps.Runspace = runspace;
            
            ps.AddStatement()
                .AddCommand("Stop-Service")
                    .AddParameter("Name", ServiceName);
            ps.AddStatement()
                .AddCommand("Remove-Service")
                    .AddParameter("Name", ServiceName);
            
            ps.AddStatement()
                .AddCommand("New-Service")
                    .AddParameter("ServiceName", ServiceName)
                    .AddParameter("DisplayName", ServiceDisplayName)
                    .AddParameter("BinaryPathName", Path.Combine(path.FullName, ExecutableFileName))
                    .AddParameter("StartupType", "Automatic");

            ps.AddStatement()
                .AddCommand("Start-Service")
                    .AddParameter("ServiceName", ServiceName);
                
            await ps.InvokeAsync();

            return ps.Streams.Error.Count > 2 // suppose errors for stop and remove when not existed
                ? Result.Failure("Error when adding agent to services", ps.Streams.Error.Select(x => x.Exception).ToList()) 
                : Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("Error when adding agent to services", ex);
        }
    }
    
    public Task<Result<string>> GetOsNameAsync(CancellationToken ct = default)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            
            if (key == null)
                return Task.FromResult(Result.Failure<string>("Registry key not found"));

            var productName = key.GetValue("ProductName")?.ToString() ?? "Windows";

            // FIX: Windows 11 often still reports "Windows 10" in the ProductName registry key.
            // We check the build number to manually correct this for display purposes.
            var currentBuild = Environment.OSVersion.Version.Build;
            if (currentBuild >= 22000 && productName.Contains("Windows 10"))
            {
                productName = productName.Replace("Windows 10", "Windows 11");
            }

            return Task.FromResult(Result.Success(productName));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure<string>($"Failed to read Windows Name: {ex.Message}"));
        }
    }

    public Task<Result<string?>> GetOsVersionAsync(CancellationToken ct = default)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            
            if (key == null)
                return Task.FromResult(Result.Failure<string?>("Registry key not found."));

            // "DisplayVersion" holds values like "22H2". 
            // On older Windows 10 versions, this was "ReleaseId".
            var displayVersion = key.GetValue("DisplayVersion")?.ToString();
            
            if (string.IsNullOrEmpty(displayVersion))
            {
                // Fallback for older Windows 10
                displayVersion = key.GetValue("ReleaseId")?.ToString();
            }

            return Task.FromResult(Result.Success(displayVersion));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure<string?>($"Failed to read Windows Version: {ex.Message}"));
        }
    }
}

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