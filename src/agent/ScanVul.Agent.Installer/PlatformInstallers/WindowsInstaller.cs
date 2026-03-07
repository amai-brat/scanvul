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
    private const string ChocoInstallScript = 
        "Set-ExecutionPolicy Bypass -Scope Process -Force; " + 
        "[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; " +
        "iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))";
        
    private const string WingetInstallScript = """
        $ProgressPreference = 'SilentlyContinue';
        Write-Output 'Checking Winget installation...';

        # 1. Define URLs (Official GitHub Releases)
        $latestReleaseUrl = 'https://api.github.com/repos/microsoft/winget-cli/releases/latest';
        try {
          $release = Invoke-RestMethod -Uri $latestReleaseUrl;
          $assetUrl = $release.assets | Where-Object { $_.name -like '*.msixbundle' } | Select-Object -ExpandProperty browser_download_url;
        } catch {
          Write-Warning 'Failed to fetch latest Winget release info.';
          return;
        }
        # 3. Install Winget Bundle
        $tempFile = [System.IO.Path]::GetTempFileName() + '.msixbundle';
        Write-Output "Downloading Winget from $assetUrl";
        Invoke-WebRequest -Uri $assetUrl -OutFile $tempFile;

        Write-Output 'Installing Winget...';
        try {
          # Provisioning allows it to be used by LocalSystem and new users
          Add-AppxProvisionedPackage -Online -PackagePath $tempFile -SkipLicense -ErrorAction Stop
        } catch {
          Write-Warning "Standard provisioning failed. Attempting local registration...";
          # Fallback for some OS versions
          Add-AppxPackage -Path $tempFile
        }

        Remove-Item $tempFile -Force;
        """;
    
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
                .AddParameter("Name", ServiceName)
                .AddParameter("ErrorAction", "SilentlyContinue");
            
            ps.AddStatement()
                .AddCommand("Remove-Service")
                .AddParameter("Name", ServiceName)
                .AddParameter("ErrorAction", "SilentlyContinue");
                
            Console.WriteLine("Installing chocolatey (warnings about previous installation can be ignored)...");
            ps.AddStatement()
                .AddScript(ChocoInstallScript);
            
            Console.WriteLine("Bootstrapping Winget...");
            ps.AddStatement()
                .AddScript(WingetInstallScript);

            await ps.InvokeAsync();

            foreach (var record in ps.Streams.Information)
                Console.WriteLine($"\t[INFO] {record.MessageData.ToString()?.ReplaceLineEndings($"{Environment.NewLine}\t")}");
            
            foreach (var record in ps.Streams.Warning)
                Console.WriteLine($"\t[WARNING] {record.Message.ReplaceLineEndings($"{Environment.NewLine}\t")}");
            
            return ps.Streams.Error.Count > 0 
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