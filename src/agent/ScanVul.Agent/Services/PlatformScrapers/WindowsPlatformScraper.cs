using System.Management;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.Versioning;
using ScanVul.Agent.Models;

namespace ScanVul.Agent.Services.PlatformScrapers;

[SupportedOSPlatform("windows")]
public class WindowsPlatformScraper(ILogger<WindowsPlatformScraper> logger) : IPlatformScraper
{
    // using hardcoded script because of possible: *.ps1 cannot be loaded because running scripts is disabled on this system
    private const string Script = """
      $registryPaths = @(
          "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall",
          "HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
      )

      $installedPrograms = foreach ($path in $registryPaths) {
          Get-ItemProperty -Path $path\* | Select-Object DisplayName, DisplayVersion
      }
      
      # Remove duplicates and display results
      $installedPrograms | Sort-Object -Property DisplayName -Unique
      """;
    
    public async Task<List<PackageInfo>> ScrapePackagesAsync(CancellationToken ct = default)
    {
        using var runspace = RunspaceFactory.CreateRunspace();
        // ReSharper disable once MethodHasAsyncOverload
        runspace.Open();
        
        using var ps = PowerShell.Create();
        ps.Runspace = runspace;
        ps.AddScript(Script);
        
        var packages = await ps.InvokeAsync();
        
        foreach (var error in ps.Streams.Error)
        {
            logger.LogWarning(error.Exception, "Error when scraping packages");
        }

        var result=  packages
            .Where(x => !string.IsNullOrWhiteSpace(x.Properties["DisplayName"]?.Value?.ToString()))
            .Select(result => new PackageInfo
            {
                Name = result.Properties["DisplayName"].Value.ToString()!, 
                Version = result.Properties["DisplayVersion"]?.Value?.ToString()
            }).ToList();

        return result;
    }
    
    public Task<ComputerInfo> ScrapeComputerInfoAsync(CancellationToken ct = default)
    {
        var info = new ComputerInfo
        {
            ComputerName = Environment.MachineName,
            CpuName = GetCpuNameWmi(),
            MemoryInMb = GetTotalMemoryMbWmi()
        };
        return Task.FromResult(info);
    }

    private string GetCpuNameWmi()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
            using var collection = searcher.Get();
            foreach (var obj in collection)
            {
                return obj["Name"]?.ToString() ?? "Unknown CPU";
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error when getting CPU name");
        }
        return "Unknown CPU";
    }

    private int GetTotalMemoryMbWmi()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            using var collection = searcher.Get();
            foreach (var obj in collection)
            {
                if (long.TryParse(obj["TotalPhysicalMemory"]?.ToString(), out var bytes))
                {
                    return (int)(bytes / 1024 / 1024);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error when getting amount of RAM");
        }
        return 0;
    }
}