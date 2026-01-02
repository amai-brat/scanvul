using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using ScanVul.Agent.Helpers;
using ScanVul.Agent.Services.Commands;
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
    
    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.AddKeyedSingleton<ConcurrentQueue<AgentCommand>>(Consts.KeyedServices.CommandQueue);
        services.AddTransient<ICommandHandler<ReportPackagesCommand>, ReportPackagesCommandHandler>();

        return services;
    }
}