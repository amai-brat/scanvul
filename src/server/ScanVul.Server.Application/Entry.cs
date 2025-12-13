using FastEndpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScanVul.Server.Domain.Cve.Options;
using ScanVul.Server.Domain.Cve.Services;

namespace ScanVul.Server.Application;

public static class Entry
{
    public static IServiceCollection AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFastEndpoints();
        
        services.Configure<ScanSettings>(configuration.GetSection("Scan"));
        services.AddScoped<IVulnerablePackageScanner, VulnerablePackageScanner>();
        services.AddScoped<VersionMatcher>();
        
        return services;
    }
}