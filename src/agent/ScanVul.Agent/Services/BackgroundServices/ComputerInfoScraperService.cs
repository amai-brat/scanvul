using System.Net.Http.Json;
using ScanVul.Agent.Helpers;
using ScanVul.Agent.Services.PlatformScrapers;
using ScanVul.Contracts.ComputerInfos;

namespace ScanVul.Agent.Services.BackgroundServices;

public class ComputerInfoScraperService(
    IServiceScopeFactory scopeFactory, 
    IHttpClientFactory httpClientFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MainService>>();
        var scraper = scope.ServiceProvider.GetRequiredService<IPlatformScraper>();
        var httpClient = httpClientFactory.CreateClient(Consts.HttpClientNames.Server);
            
        try
        {
            var result = await scraper.ScrapeComputerInfoAsync(stoppingToken);
            logger.LogInformation("[{Time}] Computer: {ComputerName}. CPU: {CpuInfo}. RAM: {RAM}", DateTimeOffset.Now, result.ComputerName, result.CpuName, result.MemoryInMb);
                
            var response = await httpClient.PostAsJsonAsync("api/v1/agents/computer/report", new ReportComputerInfoRequest
            {
                ComputerName = result.ComputerName,
                MemoryInMb = result.MemoryInMb,
                CpuName = result.CpuName
            }, cancellationToken: stoppingToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Error when sending computer info to server: {StatusCode} - {Message}", 
                    response.StatusCode, 
                    await response.Content.ReadAsStringAsync(stoppingToken));
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error when scraping and sending computer info at {Time}", DateTimeOffset.Now);
        }
    }
}