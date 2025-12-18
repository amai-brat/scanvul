using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ScanVul.Server.Infrastructure.Hangfire.Workers;

namespace ScanVul.Server.Infrastructure.Hangfire;

public static class Entry
{
    public static IServiceCollection AddHangfireServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var hangfireOptions = configuration
            .GetSection("Hangfire")
            .Get<HangfireOptions>();
        
        if (hangfireOptions == null || 
            string.IsNullOrWhiteSpace(hangfireOptions.ConnectionString))
            throw new InvalidOperationException("Hangfire options not set");
        
        services.Configure<HangfireOptions>(configuration.GetSection("Hangfire"));
        services.AddHangfire(conf =>
        {
            conf.UsePostgreSqlStorage(o =>
            {
                o.UseNpgsqlConnection(hangfireOptions.ConnectionString);
            });
        });
        services.AddHangfireServer();
        
        return services;
    }

    public static IApplicationBuilder UseHangfire(this IApplicationBuilder app)
    {
        app.UseHangfireDashboard();
        
        var options = app.ApplicationServices.GetRequiredService<IOptions<HangfireOptions>>();
        
        RecurringJob.AddOrUpdate<CveSnapshotDownloadWorker>(
            "cve_snapshot_download", 
            x => x.RunAsync(CancellationToken.None), 
            options.Value.CveSnapshotDownloadJobCron, new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        return app;
    }
}