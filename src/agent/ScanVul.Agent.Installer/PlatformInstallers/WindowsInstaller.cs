using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace ScanVul.Agent.Installer.PlatformInstallers;

[SupportedOSPlatform("windows")]
public class WindowsInstaller : IPlatformInstaller
{
    private const string ServiceName = "ScanVul.Agent";
    private const string ServiceDisplayName = "ScanVul Agent";
    
    public DirectoryInfo DefaultInstallationPath => new(@"C:\Program Files\ScanVul");
    public string AgentZipResourceName => "agent.win64.zip";
    public string ExecutableFileName => "ScanVul.Agent.exe";
    public async Task<Result> PrepareInstallationAsync(CancellationToken ct = default)
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
                
            await ps.InvokeAsync();

            return ps.Streams.Error.Count > 2 // suppose errors for stop and remove when not existed
                ? Result.Failure("Error when preparing installation", ps.Streams.Error.Select(x => x.Exception).ToList()) 
                : Result.Success();
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
            using var runspace = RunspaceFactory.CreateRunspace();
            // ReSharper disable once MethodHasAsyncOverload
            runspace.Open();
        
            using var ps = PowerShell.Create();
            ps.Runspace = runspace;
            
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

            return ps.Streams.Error.Count > 0
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