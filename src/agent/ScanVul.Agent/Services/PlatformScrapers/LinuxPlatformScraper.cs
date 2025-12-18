using ScanVul.Agent.Models;

namespace ScanVul.Agent.Services.PlatformScrapers;

public abstract class LinuxPlatformScraper(ILogger logger) : IPlatformScraper
{ 
    private const string CpuInfoPath = "/proc/cpuinfo";
    private const string MemInfoPath = "/proc/meminfo";
    
    public abstract Task<List<PackageInfo>> ScrapePackagesAsync(CancellationToken ct = default);
    public async Task<ComputerInfo> ScrapeComputerInfoAsync(CancellationToken ct = default)
    {
        var cpuName = await GetCpuNameFromProcAsync(ct);
        var memMb = await GetMemoryFromProcAsync(ct);

        return new ComputerInfo
        {
            ComputerName = Environment.MachineName,
            CpuName = cpuName,
            MemoryInMb = memMb
        };
    }

    private async Task<string> GetCpuNameFromProcAsync(CancellationToken ct)
    {
        if (!File.Exists(CpuInfoPath)) return "Unknown (Non-Standard Linux)";

        try
        {
            using var reader = new StreamReader(CpuInfoPath);
            while (await reader.ReadLineAsync(ct) is { } line)
            {
                if (!line.StartsWith("model name", StringComparison.OrdinalIgnoreCase)) continue;
                
                var parts = line.Split(':');
                if (parts.Length > 1)
                {
                    return parts[1].Trim();
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error when scraping CPU info");
        }

        return "Unknown CPU";
    }

    private async Task<int> GetMemoryFromProcAsync(CancellationToken ct)
    {
        if (!File.Exists(MemInfoPath)) return 0;

        try
        {
            using var reader = new StreamReader(MemInfoPath);
            while (await reader.ReadLineAsync(ct) is { } line)
            {
                if (!line.StartsWith("MemTotal", StringComparison.OrdinalIgnoreCase)) continue;
                
                var parts = line.Split(':');
                if (parts.Length <= 1) continue;
                    
                // example: 29555952 kB
                var numberPart = new string(parts[1].Trim().TakeWhile(char.IsDigit).ToArray());
                if (long.TryParse(numberPart, out var kb))
                {
                    return (int)(kb / 1024);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error when scraping RAM info");
        }

        return 0;
    }
}