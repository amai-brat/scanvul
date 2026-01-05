using System.Collections.Concurrent;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ScanVul.Agent.Helpers;
using ScanVul.Agent.Options;
using ScanVul.Contracts.Agents;

namespace ScanVul.Agent.Services.BackgroundServices;

public class HealthCheckService(
    IServiceScopeFactory scopeFactory, 
    IOptionsMonitor<TimeoutOptions> optionsMonitor,
    IHttpClientFactory httpClientFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<HealthCheckService>>();
            var httpClient = httpClientFactory.CreateClient(Consts.HttpClientNames.Server);
            var queue = scope.ServiceProvider.GetRequiredKeyedService<ConcurrentQueue<AgentCommand>>(Consts.KeyedServices.CommandQueue);

            try
            {
                logger.LogInformation("Ping server at {Time}", DateTimeOffset.Now);
                var response = await httpClient.PostAsync("api/v1/agents/ping", null, cancellationToken: stoppingToken);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Ping server response: {StatusCode} - {Message}", 
                        response.StatusCode, 
                        await response.Content.ReadAsStringAsync(stoppingToken));
                }

                var commandsResp = await response.Content.ReadFromJsonAsync<AgentCommandsResponse>(cancellationToken: stoppingToken);
                commandsResp?.Commands.ForEach(c => queue.Enqueue(c));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error when pinging server at {Time}", DateTimeOffset.Now);
            }
        
            await Task.Delay(optionsMonitor.CurrentValue.Ping, stoppingToken);
        }
    }
}