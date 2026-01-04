using System.Net.Http.Json;
using ScanVul.Agent.Helpers;
using ScanVul.Agent.Services.PlatformScrapers;
using ScanVul.Contracts.Agents;
using ScanVul.Contracts.PackageInfos;

namespace ScanVul.Agent.Services.CommandHandlers;

public class ReportPackagesCommandHandler(
    ILogger<ReportPackagesCommandHandler> logger,
    IPlatformScraper scraper,
    IHttpClientFactory httpClientFactory) : ICommandHandler<ReportPackagesCommand>
{
    public async Task<string> Handle(ReportPackagesCommand command, CancellationToken ct = default)
    {
        logger.LogInformation("Processing {Command}:{CommandId}", command.GetType().Name, command.CommandId);

        var httpClient = httpClientFactory.CreateClient(Consts.HttpClientNames.Server);
            
        try
        {
            var result = await scraper.ScrapePackagesAsync(ct);
            logger.LogInformation("Found {Count} packages at {Time}", result.Count, DateTimeOffset.Now);
                
            var response = await httpClient.PostAsJsonAsync("api/v1/agents/packages/report", new ReportPackagesRequest
            {
                Packages = result
                    .Select(x => new PackageInfoDto(x.Name, x.Version ?? "-"))
                    .ToList()
            }, cancellationToken: ct);

            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync(ct);
                logger.LogWarning("Error when sending packages to server: {StatusCode} - {Message}", 
                    response.StatusCode, 
                    msg);
                return $"Error when sending packages to server: {response.StatusCode} - {msg}";
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error when scraping and sending packages at {Time}", DateTimeOffset.Now);
            return $"Error when scraping and sending packages at {DateTimeOffset.Now}: {ex.Message}";
        }

        logger.LogInformation("Successfully scraped packages");
        return "OK";
    }
}