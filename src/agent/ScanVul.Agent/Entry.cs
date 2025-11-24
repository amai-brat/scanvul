using System.Runtime.InteropServices;
using ScanVul.Agent.Services.PackageInfoScrapers;

namespace ScanVul.Agent;

public static class Entry
{
    /// <summary>
    /// Add scraper according to OS and package manager
    /// </summary>
    /// <param name="services">Service collection</param>
    public static IServiceCollection AddScraper(this IServiceCollection services)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            services.AddTransient<IPackageInfoScraper, WindowsPackageInfoScraper>();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (File.Exists("/usr/bin/pacman")) 
            {
                services.AddTransient<IPackageInfoScraper, ArchLinuxPackageInfoScraper>();
            }
        }

        return services;
    }
}