using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}