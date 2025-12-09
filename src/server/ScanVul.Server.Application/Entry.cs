using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using ScanVul.Server.Domain.Cve.Services;

namespace ScanVul.Server.Application;

public static class Entry
{
    public static IServiceCollection AddFeatures(this IServiceCollection services)
    {
        services.AddFastEndpoints();
        
        services.AddScoped<IVulnerablePackageScanner, VulnerablePackageScanner>();
        
        return services;
    }
}