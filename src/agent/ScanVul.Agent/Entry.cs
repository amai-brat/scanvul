using System.Runtime.InteropServices;
using ScanVul.Agent.Services.CommandHandlers;
using ScanVul.Agent.Services.PlatformScrapers;
using ScanVul.Contracts.Agents;

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
            services.AddTransient<IPlatformScraper, WindowsPlatformScraper>();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (File.Exists("/usr/bin/pacman")) 
            {
                services.AddTransient<IPlatformScraper, ArchLinuxPlatformScraper>();
            }
            else if (File.Exists("/etc/altlinux-release"))
            {
                services.AddTransient<IPlatformScraper, AltLinuxPlatformScraper>();
            }
        }

        return services;
    }
    
    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.AddTransient<ICommandHandler<ReportPackagesCommand>, ReportPackagesCommandHandler>();

        return services;
    }
}