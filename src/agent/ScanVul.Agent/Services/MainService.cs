using Microsoft.Extensions.Options;
using ScanVul.Agent.Options;

namespace ScanVul.Agent.Services;

public class MainService(
    IServiceScopeFactory scopeFactory, 
    IOptionsMonitor<TimeoutOptions> optionsMonitor) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MainService>>();
        var scraper = scope.ServiceProvider.GetRequiredService<WindowsPackageInfoScraper>();

        try
        {
            var result = await scraper.ScrapeAsync(stoppingToken);
            // TODO: send to server
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error when scraping and sending packages");
        }
        
        await Task.Delay(optionsMonitor.CurrentValue.PackagesScraping, stoppingToken);
    }
}