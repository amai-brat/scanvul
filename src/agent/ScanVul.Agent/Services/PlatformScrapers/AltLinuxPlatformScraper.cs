using System.Diagnostics;
using ScanVul.Agent.Models;

namespace ScanVul.Agent.Services.PlatformScrapers;

public class AltLinuxPlatformScraper(ILogger<AltLinuxPlatformScraper> logger) : IPlatformScraper
{
    public async Task<List<PackageInfo>> ScrapePackagesAsync(CancellationToken ct = default)
    {
        var packages = new List<PackageInfo>();

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "rpm",
                Arguments = "-qa --queryformat \"%{NAME} %{VERSION}\\n\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync(ct);
            var errorTask = process.StandardError.ReadToEndAsync(ct);

            await process.WaitForExitAsync(ct);

            if (process.ExitCode != 0)
            {
                var errorMsg = await errorTask;
                logger.LogWarning("RPM command exited with code {Code}. Error: {Error}", process.ExitCode, errorMsg);
                return packages;
            }

            var output = await outputTask;
            using var reader = new StringReader(output);

            while (await reader.ReadLineAsync(ct) is { } line)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2)
                {
                    packages.Add(new PackageInfo
                    {
                        Name = parts[0],
                        Version = parts[1]
                    });
                }
                else
                {
                    packages.Add(new PackageInfo
                    {
                        Name = parts[0],
                        Version = null
                    });
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute rpm or parse package list");
        }

        return packages;
    }
}