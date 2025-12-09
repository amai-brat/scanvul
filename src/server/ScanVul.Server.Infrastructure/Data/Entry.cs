using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScanVul.Server.Domain.AgentAggregate.Repositories;
using ScanVul.Server.Domain.Common;
using ScanVul.Server.Infrastructure.Data.Repositories;

namespace ScanVul.Server.Infrastructure.Data;

public static class Entry
{
    public static IServiceCollection AddData(this IServiceCollection services, string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Postgres connection string not set");
        
        services.AddDbContext<AppDbContext>(b =>
        {
            b.UseNpgsql(connectionString);
            b.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IAgentRepository, AgentRepository>();
        services.AddScoped<IPackageInfoRepository, PackageInfoRepository>();
        services.AddScoped<IComputerRepository, ComputerRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
}