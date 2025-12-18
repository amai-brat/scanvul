using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ScanVul.Agent.Helpers;
using ScanVul.Agent.Options;
using ScanVul.Agent.Services.PlatformScrapers;
using ScanVul.Contracts.PackageInfos;

namespace ScanVul.Agent.Services.BackgroundServices;

public class MainService(
    IServiceScopeFactory scopeFactory, 
    IOptionsMonitor<TimeoutOptions> optionsMonitor,
    IHttpClientFactory httpClientFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<MainService>>();
            var scraper = scope.ServiceProvider.GetRequiredService<IPlatformScraper>();
            var httpClient = httpClientFactory.CreateClient(Consts.HttpClientNames.Server);
            
            try
            {
                var result = await scraper.ScrapePackagesAsync(stoppingToken);
                logger.LogInformation("Found {Count} packages at {Time}", result.Count, DateTimeOffset.Now);
                
                var response = await httpClient.PostAsJsonAsync("api/v1/agents/packages/report", new ReportPackagesRequest
                {
                    Packages = result
                        .Select(x => new PackageInfoDto(x.Name, x.Version ?? "-"))
                        .ToList()
                }, cancellationToken: stoppingToken);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Error when sending packages to server: {StatusCode} - {Message}", 
                        response.StatusCode, 
                        await response.Content.ReadAsStringAsync(stoppingToken));
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error when scraping and sending packages at {Time}", DateTimeOffset.Now);
            }
        
            await Task.Delay(optionsMonitor.CurrentValue.PackagesScraping, stoppingToken);
        }
    }
}