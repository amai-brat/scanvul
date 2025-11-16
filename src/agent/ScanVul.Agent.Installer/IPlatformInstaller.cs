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
    Task<Result> AddAgentToAutoStartAsync(DirectoryInfo path);
}

public class WindowsInstaller : IPlatformInstaller
{
    private const string ServiceName = "ScanVul.Agent";
    private const string ServiceDisplayName = "ScanVul Agent";
    
    public DirectoryInfo DefaultInstallationPath => new(@"C:\Program Files\ScanVul");
    public string AgentZipResourceName => "agent.win64.zip";
    public string ExecutableFileName => "ScanVul.Agent.exe";
    public async Task<Result> AddAgentToAutoStartAsync(DirectoryInfo path)
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
    public DirectoryInfo DefaultInstallationPath => new("/opt/scanvul");
    public string AgentZipResourceName => "agent.linux.zip";
    public string ExecutableFileName => "ScanVul.Agent";
    public Task<Result> AddAgentToAutoStartAsync(DirectoryInfo path)
    {
        Console.WriteLine(Environment.OSVersion.ToString());
        return Task.FromResult(Result.Success());
    }
}