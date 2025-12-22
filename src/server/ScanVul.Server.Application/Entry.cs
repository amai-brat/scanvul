using FastEndpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScanVul.Server.Application.Options;
using ScanVul.Server.Application.Services;
using ScanVul.Server.Domain.Cve.Options;
using ScanVul.Server.Domain.Cve.Services;
using ScanVul.Server.Domain.UserAggregate.Services;

namespace ScanVul.Server.Application;

public static class Entry
{
    public static IServiceCollection AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFastEndpoints();
        
        services.Configure<ScanSettings>(configuration.GetSection("Scan"));
        services.AddScoped<IVulnerablePackageScanner, VulnerablePackageScanner>();
        services.AddScoped<VersionMatcher>();

        services.Configure<JwtOptions>(options =>
        {
            configuration.GetSection("Jwt").Bind(options);
            JwtOptions.Validate(options);
        });
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtGenerator, JwtGenerator>();
        
        return services;
    }
}