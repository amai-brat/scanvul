using System.Management.Automation;
using System.Management.Automation.Runspaces;
using ScanVul.Agent.Models;

namespace ScanVul.Agent.Services.PlatformScrapers;

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
}