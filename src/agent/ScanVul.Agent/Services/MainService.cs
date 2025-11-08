namespace ScanVul.Agent.Services;

public class MainService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        
        var scraper = scope.ServiceProvider.GetRequiredService<WindowsPackageInfoScraper>();
        
        var result = await scraper.ScrapeAsync(stoppingToken);
        
    }
}