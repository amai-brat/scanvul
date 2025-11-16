using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

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
}

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
}

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
}