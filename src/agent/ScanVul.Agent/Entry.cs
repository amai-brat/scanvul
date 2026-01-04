using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using ScanVul.Agent.Helpers;
using ScanVul.Agent.Services.CommandHandlers;
using ScanVul.Agent.Services.PlatformAgentManagers;
using ScanVul.Agent.Services.PlatformScrapers;
using ScanVul.Contracts.Agents;

namespace ScanVul.Agent;

public static class Entry
{
    /// <summary>
    /// Add services according to OS and package manager
    /// </summary>
    /// <param name="services">Service collection</param>
    public static IServiceCollection AddPlatformServices(this IServiceCollection services)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            services.AddTransient<IPlatformScraper, WindowsPlatformScraper>();
            services.AddTransient<IPlatformAgentManager, WindowsPlatformAgentManager>();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (File.Exists("/usr/bin/pacman")) 
            {
                services.AddTransient<IPlatformScraper, ArchLinuxPlatformScraper>();
                services.AddTransient<IPlatformAgentManager, LinuxPlatformAgentManager>();
            }
            else if (File.Exists("/etc/altlinux-release"))
            {
                services.AddTransient<IPlatformScraper, AltLinuxPlatformScraper>();
                services.AddTransient<IPlatformAgentManager, LinuxPlatformAgentManager>();
            }
        }

        return services;
    }
    
    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.AddKeyedSingleton<ConcurrentQueue<AgentCommand>>(Consts.KeyedServices.CommandQueue);
        services.AddTransient<ICommandHandler<ReportPackagesCommand>, ReportPackagesCommandHandler>();
        services.AddTransient<ICommandHandler<DisableAgentCommand>, DisableAgentCommandHandler>();

        return services;
    }
}