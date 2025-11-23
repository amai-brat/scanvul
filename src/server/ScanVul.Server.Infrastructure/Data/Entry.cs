using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScanVul.Server.Domain.Repositories;
using ScanVul.Server.Infrastructure.Data.Repositories;

namespace ScanVul.Server.Infrastructure.Data;

public static class Entry
{
    public static IServiceCollection AddData(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(b =>
        {
            b.UseNpgsql(connectionString);
            b.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IAgentRepository, AgentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
}